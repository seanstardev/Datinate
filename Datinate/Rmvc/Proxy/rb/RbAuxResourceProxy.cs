using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using RMVC;
using System.Diagnostics;

namespace com.RADIO.Datinate.RMVC
{
    public class RbAuxResourceProxy : RModel
    {
        public ILookupSet[] LoadLookups(
            IReadOnlyCollection<RadioSourceDTO> sources,
            Action<int, string> progressEvt,
            Func<string, DatVO?> fetchDat,
            out Dictionary<string, DatVO> sourceIdDatDictionary)
        {
            var syncCtx = SynchronizationContext.Current;

            void Report(int pct, string msg)
            {
                if (syncCtx == null)
                {
                    progressEvt?.Invoke(pct, msg);
                    return;
                }

                syncCtx.Post(_ => progressEvt?.Invoke(pct, msg), null);
            }

            sourceIdDatDictionary = [];

            var srcArr = sources?.ToArray() ?? Array.Empty<RadioSourceDTO>();

            List<ILookupSet> list = new List<ILookupSet>();

            for (int i = 0; i < srcArr.Length; i++)
            {
                var src = srcArr[i];
                
                Report(
                    DatinateHelper.GetReadablePercentageInt(srcArr.Length, i),
                    "Reading DAT: " + src.Id
                );

                var dat = fetchDat.Invoke(src.DatFullpath);

                if (dat == null)
                {
                    Debug.WriteLine(this + ": WARNING: This DAT Could NOT be loaded: " + src.DatFullpath);
                    continue;
                }

                var lookupsDic = new Dictionary<string, string>();

                foreach (var datEntry in dat.Entries)
                {
                    // TODO: This may need looking at... MAME names aren't path safe.
                    // UPDATE: But - Resource DATs always have path safe entries? Discuss.
                    var name = datEntry.Name.Replace("\\", "/");
                    lookupsDic.Add(name, name);
                }
                list.Add(new ResourceLookupSet(src.Id, lookupsDic, src));

                _ = sourceIdDatDictionary.TryAdd(
                    src.Id, 
                    dat);
            }

            return list.ToArray();
        }
    }
}
