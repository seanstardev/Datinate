using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;

namespace Datinate.Shared.Util
{
    public static class DatinateFamilyConverter
    {
        public static IEnumerable<BaseGameFamily> Convert(
            IEnumerable<GameFamilyVO> familyVOs,
            out Dictionary<string, CurationPartReport> reportDic)
        {
            reportDic = new Dictionary<string, CurationPartReport>();
            
            var list = new List<BaseGameFamily>();

            foreach (var vo in familyVOs)
            {
                list.Add(new BaseGameFamily(
                    vo.GetFamilyDisplayName(),
                    Convert(vo.Games, reportDic),
                    Array.Empty<IDescriptor>(),
                    BaseResourceCollection.Empty,
                    false,
                    vo.GetComment()));
            }

            return list;
        }
        public static Dictionary<string, CurationPartReport> CreatePartReportsDictionary(IEnumerable<GameFamilyVO> families)
        {
            Dictionary<string, CurationPartReport> reportDic = new Dictionary<string, CurationPartReport>();

            foreach (var family in families)
            {
                var games = family.Games;
                foreach (var game in games)
                {
                    var parts = game.Parts;
                    foreach (var part in parts)
                    {
                        reportDic[part.RomsFingerprint] = new CurationPartReport(
                        part.IsShallowPartReference,
                        part.MembershipStatusEnum);
                    }
                }
            }
            return reportDic;
        }

        private static BaseGame[] Convert(GameVO[] gameVOs, Dictionary<string, CurationPartReport> reportDic)
        {
            var list = new List<BaseGame>();

            foreach (var vo in gameVOs)
            {
                list.Add(new BaseGame(
                    vo.GetNameWithoutExt(),
                    Convert(vo.Parts, reportDic),
                    vo.GetLaunchName()));
            }

            return list.ToArray();
        }

        private static BaseGamePart[] Convert(GamePartVO[] partVOs, Dictionary<string, CurationPartReport> reportDic)
        {
            var list = new List<BaseGamePart>();

            foreach (var vo in partVOs)
            {
                list.Add(new BaseGamePart(
                    vo.GetName(),
                    ConvertAliases(vo.GetAliases()),
                    vo.GetChecksums(),
                    vo.GetDirectoryId(),
                    vo.GetDisplayName(),
                    vo.LaunchName,
                    vo.Tag)
                { 
                    Exclude = vo.Exclude
                });

                reportDic[vo.RomsFingerprint] = new CurationPartReport(
                    vo.IsShallowPartReference,
                    vo.MembershipStatusEnum);
            }

            return list.ToArray();
        }

        private static BaseGamePart[] ConvertAliases(GamePartVO[] aliasVOs)
        {
            var list = new List<BaseGamePart>();

            foreach (var vo in aliasVOs)
            {
                list.Add(new BaseGamePart(
                    vo.GetName(),
                    Array.Empty<IGamePart>(),
                    vo.GetChecksums(),
                    vo.GetDirectoryId(),
                    vo.GetDisplayName(),
                    vo.LaunchName,
                    vo.Tag));
            }

            return list.ToArray();
        }
    }
}
