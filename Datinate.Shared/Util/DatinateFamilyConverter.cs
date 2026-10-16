using Datinate.Shared.Radio;

namespace Datinate.Shared
{
    public static class DatinateFamilyConverter
    {
        public static IEnumerable<BaseGameFamily> Convert(
            IEnumerable<GameFamilyVO> familyVOs,
            out Dictionary<string, CurationPartReport> reportDic,
            Action<GamePartVO, BaseGamePart>? onPartConverted = null)
        {
            reportDic = new Dictionary<string, CurationPartReport>();
            
            var list = new List<BaseGameFamily>();

            foreach (var vo in familyVOs)
            {
                list.Add(new BaseGameFamily(
                    vo.GetFamilyDisplayName(),
                    Convert(vo.Games, reportDic, onPartConverted),
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

        private static BaseGame[] Convert(GameVO[] gameVOs, Dictionary<string, CurationPartReport> reportDic,
            Action<GamePartVO, BaseGamePart>? onPartConverted)
        {
            var list = new List<BaseGame>();

            foreach (var vo in gameVOs)
            {
                list.Add(new BaseGame(
                    vo.GetNameWithoutExt(),
                    Convert(vo.Parts, reportDic, onPartConverted),
                    vo.GetLaunchName()));
            }

            return list.ToArray();
        }

        private static BaseGamePart[] Convert(GamePartVO[] partVOs, Dictionary<string, CurationPartReport> reportDic,
            Action<GamePartVO, BaseGamePart>? onPartConverted)
        {
            var list = new List<BaseGamePart>();

            foreach (var vo in partVOs)
            {
                var converted = new BaseGamePart(
                    vo.GetName(),
                    ConvertAliases(vo.GetAliases(), onPartConverted),
                    vo.GetChecksums(),
                    vo.GetDirectoryId(),
                    vo.GetDisplayName(),
                    vo.LaunchName,
                    vo.Tag)
                { 
                    Exclude = vo.Exclude
                };

                list.Add(converted);
                onPartConverted?.Invoke(vo, converted);

                reportDic[vo.RomsFingerprint] = new CurationPartReport(
                    vo.IsShallowPartReference,
                    vo.MembershipStatusEnum);
            }

            return list.ToArray();
        }

        private static BaseGamePart[] ConvertAliases(GamePartVO[] aliasVOs,
            Action<GamePartVO, BaseGamePart>? onPartConverted)
        {
            var list = new List<BaseGamePart>();

            foreach (var vo in aliasVOs)
            {
                var converted = new BaseGamePart(
                    vo.GetName(),
                    Array.Empty<IGamePart>(),
                    vo.GetChecksums(),
                    vo.GetDirectoryId(),
                    vo.GetDisplayName(),
                    vo.LaunchName,
                    vo.Tag);

                list.Add(converted);
                onPartConverted?.Invoke(vo, converted);
            }

            return list.ToArray();
        }
    }
}
