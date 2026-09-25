using datinate.app;
using Datinate.Properties;
using System.ComponentModel;
using System.Text.RegularExpressions;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class DatinateHelper 
    {
        public static readonly IReadOnlySet<MEDIA_TYPE_ENUM> ScoringMediaMasterSet = new HashSet<MEDIA_TYPE_ENUM>()
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
        };

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


        public static Bitmap? GetMediaIconBmp(MEDIA_TYPE_ENUM mediaTypeEnum)
        {
            return mediaTypeEnum switch
            {
                MEDIA_TYPE_ENUM.NOT_SET => null,
                MEDIA_TYPE_ENUM.Advert => Resources.media_icons_Advert,

                //MEDIA_TYPE_ENUM.Artwork or
                //MEDIA_TYPE_ENUM.Samples => Resources.media_icons_Support,

                MEDIA_TYPE_ENUM.Box or
                MEDIA_TYPE_ENUM.Box_Back or
                MEDIA_TYPE_ENUM.Box_Bottom or
                MEDIA_TYPE_ENUM.Box_Inlay or
                MEDIA_TYPE_ENUM.Box_Side or
                MEDIA_TYPE_ENUM.Box_Top => Resources.media_icons_Box,

                //MEDIA_TYPE_ENUM.Cabinet or
                //MEDIA_TYPE_ENUM.Control_Panel or
                //MEDIA_TYPE_ENUM.Flyer or
                //MEDIA_TYPE_ENUM.Marquee or
                //MEDIA_TYPE_ENUM.Pcb => Resources.media_icons_Box,

                MEDIA_TYPE_ENUM.Info => Resources.media_icons_Web_info,
                MEDIA_TYPE_ENUM.Info_About => Resources.media_icons_Web_Info_description,
                MEDIA_TYPE_ENUM.Info_Credits => Resources.media_icons_Web_Info_credits,
                MEDIA_TYPE_ENUM.Info_Releases => Resources.media_icons_Web_Info_releases,
                MEDIA_TYPE_ENUM.Thumb => Resources.media_icons_Icon,

                MEDIA_TYPE_ENUM.Manual or
                MEDIA_TYPE_ENUM.Manual_Front or
                MEDIA_TYPE_ENUM.Manual_Back => Resources.media_icons_Manual,

                MEDIA_TYPE_ENUM.Media or
                MEDIA_TYPE_ENUM.Media_Back or
                MEDIA_TYPE_ENUM.Media_Label or
                MEDIA_TYPE_ENUM.Media_Top => Resources.media_icons_Media_cartridge_2,

                MEDIA_TYPE_ENUM.Other or
                //ALL_TYPES_ENUM.Other_Advertisement or
                MEDIA_TYPE_ENUM.Other_Hardware or
                MEDIA_TYPE_ENUM.Other_Overlay or
                MEDIA_TYPE_ENUM.Other_Reference_Card => Resources.media_icons_Other,

                MEDIA_TYPE_ENUM.Other_Map => Resources.media_icons_Map,

                //ALL_TYPES_ENUM.Screen or
                MEDIA_TYPE_ENUM.Snap or
                MEDIA_TYPE_ENUM.Title => Resources.media_icons_Snap,

                MEDIA_TYPE_ENUM.Soundtrack => Resources.media_icons_Soundtrack,

                MEDIA_TYPE_ENUM.Video => Resources.media_icons_Video,

                _ => null
            };
        }
        
        
        // TODO: DatGrouper Title stuff that needs ironing out
        //
        //
        private static Font? titleUiFont;
        private static bool titleQueuedInitialised = false;
        private static bool titleAutomatedInitialised = false;
        private static bool titleCuratedInitialised = false;
        private static bool titleMediaInitialised = false;

        public static void SetTitleQueued(DatGrouperTitleUI titleUI)
        {
            if (titleUI.Text == "Queued" && titleQueuedInitialised) return;
            titleQueuedInitialised = true;
            SetTitleBase(titleUI, PseudoAutoColour);
            titleUI.Text = "Queued";
        }

        public static void SetTitleAutomated(DatGrouperTitleUI titleUI)
        {
            if (titleUI.Text == "Automated" && titleAutomatedInitialised) return;
            titleAutomatedInitialised = true; 
            SetTitleBase(titleUI, PseudoAutoColour);
            titleUI.Text = "Automated";
        }
        public static void SetTitleCurated(DatGrouperTitleUI titleUI)
        {
            if (titleUI.Text == "Curated" && titleCuratedInitialised) return;
            titleCuratedInitialised = true;
            SetTitleBase(titleUI, Color.LightGreen);
            titleUI.Text = "Curated";
        }
        public static void SetTitleMedia(DatGrouperTitleUI titleUI)
        {
            if (titleUI.Text == "Media" && titleMediaInitialised) return;
            titleMediaInitialised = true;
            SetTitleBase(titleUI, Color.IndianRed);
            titleUI.Text = "Media";
        }
        private static void SetTitleBase(DatGrouperTitleUI titleUI, Color colour)
        {
            titleUI.BaseColor = colour;
            titleUI.Size = new Size(172, 31);
            titleUI.RightTextPadding = new Padding(10, 2, 10, 2);
            titleUI.LeftTextPadding = new Padding(10, 2, 10, 2);
            titleUI.Font = GetTitleUiFont();
            titleUI.TabStop = false;
        }
        private static readonly Color PseudoAutoColour = Color.FromArgb(192, 192, 255);
        private static Font GetTitleUiFont()
        {
            if (titleUiFont == null)
            {
                try { titleUiFont = new Font("Segoe UI Semibold", 12f, FontStyle.Bold, GraphicsUnit.Point); }
                catch { titleUiFont = new Font("Segoe UI", 12f, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point); }
            }
            return titleUiFont;
        }
    }
}
