using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Rmvc.Proxy.delegates;
using Datinate.Shared.Rb;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
using RMVC;
using System.Diagnostics;
using System.Xml;

namespace com.RADIO.Datinate.RMVC
{
    public class RbAuxMediaProxy : RModel
    {
        public MediaLookupSet[] LoadMediaAsync(
            IReadOnlyCollection<RadioSourceDTO> sources,
            Action<int, string>? progressEvt,
            string? mameHashPath,
            Func<string, DatVO?> fetchDat,
            out Dictionary<string, DatVO> sourceIdDatDictionary)
        {
            var syncCtx = SynchronizationContext.Current;

            sourceIdDatDictionary = [];

            void Report(int pct, string msg)
            {
                if (progressEvt == null)
                    return;

                if (syncCtx == null)
                {
                    progressEvt(pct, msg);
                    return;
                }

                syncCtx.Post(_ => progressEvt(pct, msg), null);
            }

            var srcArr = sources?.ToArray() ?? Array.Empty<RadioSourceDTO>();
            var mediaVOs = new MediaLookupSet[srcArr.Length];


            for (int i = 0; i < srcArr.Length; i++)
            {
                try
                {
                    var src = srcArr[i];

                    var source = src.Source ?? string.Empty;

                    Report(
                        DatinateHelper.GetReadablePercentageInt(srcArr.Length, i),
                        i + " / " + srcArr.Length + ": Reading: " + src.Id
                    );

                    var mameNameRealNameDic =
                        src.DatGroupEnum == DAT_GROUP_ENUM.MAME_MEDIA && !string.IsNullOrWhiteSpace(mameHashPath)
                            ? BuildMameDictionary(mameHashPath, src.DatSubset)
                            : new Dictionary<string, string>();

                    var vo = fetchDat.Invoke(src.DatFullpath);
                    
                    if (vo == null)
                    {
                        Debug.WriteLine(this + ": WARNING: This DAT Could NOT be loaded: " + src.DatFullpath);
                        continue;
                    }

                    _ = sourceIdDatDictionary.TryAdd(src.Id, vo);

                    var nameDatEntryNameDic = new Dictionary<string, string>();

                    var hasSubset =
                        src.DatSubset != null &&
                        !string.IsNullOrWhiteSpace(src.DatSubset.Entry);

                    if (hasSubset)
                    {
                        AddSubsetLookupEntries(
                            vo.Entries,
                            src.DatSubset!,
                            mameNameRealNameDic,
                            nameDatEntryNameDic);
                    }
                    else
                    {
                        foreach (var datEntry in vo.Entries)
                        {
                            var entryName = datEntry.Name.Replace("\\", "/");

                            if (datEntry.Roms.Length == 1 &&
                                Path.GetFileNameWithoutExtension(datEntry.Roms[0].Name) == datEntry.Name)
                            {
                                nameDatEntryNameDic.Add(entryName, datEntry.Roms[0].Name.Replace("\\", "/"));
                            }
                            else
                            {
                                nameDatEntryNameDic.Add(entryName, entryName);
                            }
                        }
                    }

                    mediaVOs[i] = new MediaLookupSet(
                        src.Id,
                        nameDatEntryNameDic,
                        source,
                        src
                    );
                }
                catch (Exception e)
                {
                    Debug.WriteLine(nameof(RbAuxMediaProxy) + ": " + e);
                }
            }

            return mediaVOs;
        }

        private static void AddSubsetLookupEntries(
            IReadOnlyCollection<DatGameVO> datEntries,
            DatSubsetFilter datSubsetFilter,
            IReadOnlyDictionary<string, string> mameNameRealNameDic,
            Dictionary<string, string> nameDatEntryNameDic)
        {
            if (string.IsNullOrWhiteSpace(datSubsetFilter.Entry))
                return;

            DatGameVO? datEntry = null;

            foreach (var entry in datEntries)
            {
                if (entry.Name == datSubsetFilter.Entry)
                {
                    datEntry = entry;
                    break;
                }
            }

            if (datEntry == null)
                return;

            if (!datSubsetFilter.HasFolder)
            {
                foreach (var rom in datEntry.Roms)
                    AddSubsetRomLookup(rom.Name, mameNameRealNameDic, nameDatEntryNameDic);

                return;
            }

            var part = datSubsetFilter.Path + @"\";

            foreach (var rom in datEntry.Roms)
            {
                if (!rom.Name.StartsWith(part))
                    continue;

                var trimmedRomName = rom.Name.Substring(part.Length);
                AddSubsetRomLookup(trimmedRomName, mameNameRealNameDic, nameDatEntryNameDic);
            }
        }
        private static void AddSubsetRomLookup(
            string rawRomName,
            IReadOnlyDictionary<string, string> mameNameRealNameDic,
            Dictionary<string, string> nameDatEntryNameDic)
        {
            var mameName = Path.ChangeExtension(rawRomName, null);
            var lookupName = mameNameRealNameDic.TryGetValue(mameName, out var realName)
                ? realName
                : mameName;

            lookupName = lookupName.Replace("\\", "/");

            var normalisedRomName = rawRomName.Replace("\\", "/");

            if (!nameDatEntryNameDic.TryAdd(lookupName, normalisedRomName))
                nameDatEntryNameDic.Add(rawRomName, normalisedRomName);
        }


        private static Dictionary<string, string> BuildMameDictionary(string mameHashPath, DatSubsetFilter? datSubsetFilter)
        {
            var dic = new Dictionary<string, string>();

            if (datSubsetFilter == null || !datSubsetFilter.HasFolder)
                return dic;

            var hashFullpath = Path.Combine(mameHashPath, datSubsetFilter.Path + ".xml");

            if (!File.Exists(hashFullpath))
                return dic;

            try
            {
                var doc = new XmlDocument();
                doc.Load(hashFullpath);

                var mameDat = DatSoftHelper.GetDat(hashFullpath, doc.OuterXml);

                foreach (var entry in mameDat!.Entries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.MameLaunchName))
                        _ = dic.TryAdd(entry.MameLaunchName, entry.Name.Replace("\\", "/"));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debug.WriteLine(mameHashPath);
                Debug.WriteLine(datSubsetFilter.Entry + " -- " + datSubsetFilter.Path);
            }

            return dic;
        }
    }
}
