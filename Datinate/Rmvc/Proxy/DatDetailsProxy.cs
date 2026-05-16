using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Rmvc.Proxy.delegates;
using RMVC;
using System.Diagnostics;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class DatDetailsProxy : RModel
    {
        private readonly Dictionary<string, DatVO> datFullpathDatCache = new Dictionary<string, DatVO>();

        public void ClearDatCache()
        {
            datFullpathDatCache.Clear();
        }
        public DatVO ApplyDatSubfilters(DatVO datVO, DatSubsetFilter? subset)
        {
            if (subset == null || string.IsNullOrWhiteSpace(subset.Entry))
                return datVO;

            if (datVO.Entries.Count == 0)
                return datVO;

            DatGameVO? src;

            if (datVO.Entries.Count == 1)
            {
                src = datVO.Entries.FirstOrDefault();
            }
            else
            {
                src = null;

                foreach (var e in datVO.Entries)
                {
                    if (string.Equals(e.Name, subset.Entry, StringComparison.Ordinal))
                    {
                        src = e;
                        break;
                    }
                }
            }

            if (src == null || src.Roms == null || src.Roms.Length == 0)
                return datVO;

            var outGames = new DatGameVO[src.Roms.Length];

            ulong totalSize = 0;
            for (var i = 0; i < src.Roms.Length; i++)
            {
                var rom = src.Roms[i];
                totalSize += rom.Size;

                var g = new DatGameVO(
                    rom.Name,
                    src.Description,
                    src.Publisher,
                    src.Region,
                    src.Date,
                    src.Category,
                    src.MameLaunchName,
                    src.ParentName,
                    new[] { rom },
                    src.ParentName);

                g.ExpressionActionEnum = src.ExpressionActionEnum;

                outGames[i] = g;
            }

            return new DatVO(datVO.DatFullpath, datVO.DatHeaderVO, outGames, datVO.RawDatText)
            {
                AlwaysOneRomPerGame = outGames.Length > 0,
                HasPCloneStructure = false,
                ContainsChds = datVO.ContainsChds
            };
        }

        public DatVO? GetDat(string datFullpath)
        {
            return GetDatInternal(datFullpath, out var _, null, true);
        }
        public DatVO? GetDat(string datFullpath, DAT_FORMAT_ENUM datFormatEnum)
        {
            return GetDatInternal(datFullpath, out var _, datFormatEnum, true);
        }
        public DatVO? GetDat(string datFullpath, out RawFileData rawFileData)
        {
            return GetDatInternal(datFullpath, out rawFileData, null, false);
        }

        private DatVO? GetDatInternal(string datFullpath, out RawFileData? rawFileData, DAT_FORMAT_ENUM? datFormatEnum, bool skipChecksumBuilding)
        {
            rawFileData = null;

            if (datFullpathDatCache.ContainsKey(datFullpath))
                return datFullpathDatCache[datFullpath];

            if (datFormatEnum != null)
            {
                var result = GetDatInternal(datFullpath, datFormatEnum);
                if (result != null)
                    return result;
            }

            rawFileData = DatLoadingHelper.GetFileData(datFullpath, skipChecksumBuilding);
            string datRawText = rawFileData.RawText;

            var datSoftlist = LoadMameSoftlistDat(datFullpath, datRawText);
            if (datSoftlist != null) { return datSoftlist; }

            var datDatafile = LoadDatafileDat(datFullpath, datRawText);
            if (datDatafile != null) { return datDatafile; }

            var datClr = LoadClrDat(datFullpath, datRawText);
            if (datClr != null) { return datClr; }

            var datMame = LoadMameListxmlDat(datFullpath, datRawText);
            if (datMame != null) { return datMame; }
            
            var datTdc = LoadTdcDat(datFullpath, datRawText);
            if (datTdc != null) { return datTdc; }

            Debug.WriteLine("WARNING: Could not load DAT: '" + datFullpath + "'.");

            return null;
        }

        private DatVO? GetDatInternal(string datFullpath, DAT_FORMAT_ENUM? datFormatEnum)
        {
            if (datFullpathDatCache.ContainsKey(datFullpath))
                return datFullpathDatCache[datFullpath];

            string text = DatLoadingHelper.LoadDat(datFullpath);

            DatVO? datVO;

            switch (datFormatEnum)
            {
                case DAT_FORMAT_ENUM.ClrMamePro:
                    datVO = LoadClrDat(datFullpath, text)!;
                    break;

                case DAT_FORMAT_ENUM.LogiqxXml:
                    datVO = LoadDatafileDat(datFullpath, text)!;
                    break;

                case DAT_FORMAT_ENUM.MameSoftwareListXml:
                    datVO = LoadMameSoftlistDat(datFullpath, text)!;
                    break;

                case DAT_FORMAT_ENUM.MameListXml:
                    datVO = LoadMameListxmlDat(datFullpath, text)!;
                    break;

                case DAT_FORMAT_ENUM.DosCenter:
                    datVO = LoadTdcDat(datFullpath, text);
                    break;

                default:
                    Debug.Print(this + ": ERROR: Unknown DatType: " + Path.GetFileName(datFullpath));
                    return null;

            }
            return datVO;
        }

        private DatVO? LoadMameSoftlistDat(string datFullpath, string datRawText)
        {
            var dat = DatSoftHelper.GetDat(datFullpath, datRawText);
            if (dat != null)
            {
                Debug.WriteLine(">> LOAD OK: LoadMameSofListDat: " + datFullpath);
                datFullpathDatCache[datFullpath] = dat;
            }
            return dat;
        }
        private DatVO? LoadMameListxmlDat(string datFullpath, string datRawText)
        {
            var dat = DatMameXmlHelper.GetDat(datFullpath, datRawText);
            if (dat != null)
            {
                Debug.WriteLine(">> LOAD OK: LoadMameListxmlDat: " + datFullpath);
                datFullpathDatCache[datFullpath] = dat;
            }
            return dat;
        }
        private DatVO? LoadTdcDat(string datFullpath, string datRawText)
        {
            var dat = DatTdcHelper.GetDat(datFullpath, datRawText);
            if (dat != null)
            {
                Debug.WriteLine(">> LOAD OK: LoadTdcDat: " + datFullpath);
                datFullpathDatCache[datFullpath] = dat;
            }
            return dat;
        }
        private DatVO? LoadDatafileDat(string datFullpath, string datRawText)
        {
            var dat = NoIntroHelper.GetDat(datFullpath, datRawText);
            if (dat != null)
            {
                Debug.WriteLine(">> LOAD OK: LoadDataFileDat: " + datFullpath);
                datFullpathDatCache[datFullpath] = dat;
            }
            return dat;
        }
        private DatVO? LoadClrDat(string datFullpath, string datRawText)
        {
            var dat = DatClrHelper.GetDat(datFullpath, datRawText);
            if (dat != null)
            {
                Debug.WriteLine(">> LOAD OK: LoadDClrDat: " + datFullpath);
                datFullpathDatCache[datFullpath] = dat;
            }
            return dat;
        }


        protected override void Initialise()
        {

        }
    }
}
