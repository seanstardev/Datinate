using Datinate.Shared.Dat;
using System.ComponentModel;
using System.Text.RegularExpressions;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Shared
{
    public static class DatinateHelper 
    {
        public static readonly IReadOnlySet20<MEDIA_TYPE_ENUM> ScoringMediaMasterSet =
            new ReadOnlySet20<MEDIA_TYPE_ENUM>(new HashSet<MEDIA_TYPE_ENUM>
                {
                    MEDIA_TYPE_ENUM.Advert,
                    MEDIA_TYPE_ENUM.Box,
                    MEDIA_TYPE_ENUM.Box_Back,
                    MEDIA_TYPE_ENUM.Box_Bottom,
                    MEDIA_TYPE_ENUM.Box_Inlay,
                    MEDIA_TYPE_ENUM.Box_Side,
                    MEDIA_TYPE_ENUM.Box_Top,
                    MEDIA_TYPE_ENUM.Info,
                    MEDIA_TYPE_ENUM.Manual,
                    MEDIA_TYPE_ENUM.Media,
                    MEDIA_TYPE_ENUM.Media_Back,
                    MEDIA_TYPE_ENUM.Media_Label,
                    MEDIA_TYPE_ENUM.Media_Top,
                    MEDIA_TYPE_ENUM.Other,
                    MEDIA_TYPE_ENUM.Other_Map,
                    MEDIA_TYPE_ENUM.Snap,
                    MEDIA_TYPE_ENUM.Soundtrack,
                    MEDIA_TYPE_ENUM.Thumb,
                    MEDIA_TYPE_ENUM.Title,
                    MEDIA_TYPE_ENUM.Video
                });

        public static bool IsMameOrMameSoftlist(DAT_GROUP_ENUM datGroupEnum)
        {
            return datGroupEnum is DAT_GROUP_ENUM.MAME_SL or DAT_GROUP_ENUM.MAME;
        }

        private static string individualFlagsRegex = @"(\[[^\[]+\])|(\([^\(]+\))";

        public const int TeeViewHorizontalOffset = 28;

        public const string WEB_BROWSER_MAIN_SearchGame = "WEB_BROWSER_MAIN_b8c7d6e5-f4a3-4b2c-9d1e-0f1a2b3c4d5e";
        public const string WEB_BROWSER_MEDIA_ShowMedia = "WEB_BROWSER_MEDIA_9b7a6c2e-1d0a-4c5d-91c0-7d1f0d2e8d87";
        public const string WEB_BROWSER_MEDIA_ShowMedia_2 = "WEB_BROWSER_MEDIA_2_9b7a6c2e-1d0a-4c5d-91c0-7d1f0d2e8d87";
        public const string WEB_BROWSER_MEDIA_ShowMediaCard = "WEB_BROWSER_MEDIA_337a6c2e-1d0a-4c5d-91c0-7d1f0d2e8d87";
        public static bool IsMediaLayout(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum)
        {
            return layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Curated_Assign
                || layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Auto_ReadOnly;
        }
        
        public static bool IsDebugBuild { get { 
            #if DEBUG
                return true;
            #else
                return false;
            #endif
            }
        }
        public static string GetDateTimeNowUtcString()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static bool IsDesignTime =>
            LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        
        
        /// <summary>
        /// For showing the parts of MAME names
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string GetDisplayNameWithPartOwner(DatGameVO game)
        {
            string entryName = game.Name;
            if (!string.IsNullOrWhiteSpace(game.PartOwnerName))
                entryName += $" <{game.PartOwnerName}>";

            return entryName;
        }
        
        public static int GetReadablePercentageInt(int max, int part)
        {
            return Convert.ToInt32(((double)part / (double)max) * 100);
        }

        public static string CreateGameFamilyDisplayName(string flaglessName, string publisherOrRegion)  =>
            flaglessName + " (" + publisherOrRegion + ")";

        public static TEnum GetEnumFromString<TEnum>(string strEnumValue, TEnum fallbackEnum)
        {
            if (!GetEnumExists<TEnum>(strEnumValue))
                return fallbackEnum;

            return GetEnumFromString<TEnum>(strEnumValue);
        }

        public static TEnum GetEnumFromString<TEnum>(string strEnumValue)
        {
            if (!GetEnumExists<TEnum>(strEnumValue))
            {
                System.Diagnostics.Debug.WriteLine("----- ERROR: >" + strEnumValue + "<, enum: " + typeof(TEnum));
                throw new Exception("GetEnumFromString() ERROR: >" + strEnumValue + "<");
            }

            var s = strEnumValue.Trim();
            return (TEnum)System.Enum.Parse(typeof(TEnum), s, true);
        }

        public static bool GetEnumExists<TEnum>(string strEnumValue)
        {
            if (string.IsNullOrWhiteSpace(strEnumValue))
                return false;

            var s = strEnumValue.Trim();

            var names = System.Enum.GetNames(typeof(TEnum));
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(names[i], s, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
        private static readonly Regex BalancedGroupsRegex =
            new Regex(@"\[[^\]]*\]|\([^\)]*\)", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex TrailingUnmatchedOpenRegex =
            new Regex(@"\[[^\]]*$|\([^\)]*$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex MultiWhitespaceRegex =
            new Regex(@"\s{2,}", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static string UnBracket(string gameName)
        {
            var s = BalancedGroupsRegex.Replace(gameName, " ");
            s = TrailingUnmatchedOpenRegex.Replace(s, " ");
            s = MultiWhitespaceRegex.Replace(s, " ").Trim();

            return s.Length == 0 ? gameName : s;
        }
        public static string GetFlaglessName(string gameName) => UnBracket(gameName);
        
        public static string[] GetIndividualFlags(string gameName) 
        {
            List<string> list = new List<string>();

            MatchCollection mc = Regex.Matches(gameName, individualFlagsRegex);

            for (int i = 0; i < mc.Count; i++) {
                list.Add(mc[i].Value);
            }
            return list.ToArray();
        }

        public static string GetReadableNumber(int nbr) => nbr.ToString("N0");
        public static string GetReadableNumber(long nbr) => nbr.ToString("N0");
        public static string GetReadableNumber(ulong nbr) => nbr.ToString("N0");

        public static string GetReadableNumber(decimal nbr, int decimals)
        {
            if ((uint)decimals > 28u) decimals = 28;
            return nbr.ToString("N" + decimals);
        }
    }
}
