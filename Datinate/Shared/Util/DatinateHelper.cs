using datinate.app;
using Datinate.Properties;
using RadioLibCore.RadioDat;
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
            return layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Auto_Assign
                || layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Curated_Assign
                || layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Auto_ReadOnly
                || layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Curated_ReadOnly;
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
        
        public static bool ShowDialogYesNo(string message, string title = "Attention")
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }


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
        public static string? GetDatFriendlyName(string? pointerId)
        {
            if (string.IsNullOrWhiteSpace(pointerId))
                return null;

            var input = pointerId.Trim();

            string? label = null;

            int dash = input.IndexOf(" - ", StringComparison.Ordinal);
            if (dash >= 0)
                label = input.Substring(dash + 3).Trim();

            if (string.IsNullOrWhiteSpace(label))
                return null;

            var s = label.Trim();

            int paren = s.IndexOf('(');
            if (paren >= 0)
                s = s.Substring(0, paren).TrimEnd();

            int firstColon = s.IndexOf(':');
            if (firstColon >= 0)
            {
                var prefix = s.Substring(0, firstColon).Trim();

                if (IsAlphaWord(prefix))
                {
                    if (prefix.Length <= 4)
                        s = prefix.ToUpperInvariant();
                    else if (prefix.Equals("Other", StringComparison.OrdinalIgnoreCase))
                        s = "Other";
                    else
                        s = TakeAfterLastColon(s);
                }
                else
                {
                    s = TakeAfterLastColon(s);
                }
            }

            s = s.Replace('-', ' ').Replace('_', ' ').Trim();

            if (s.Length == 0)
                return null;

            if (!IsDatFriendlyNameAcceptable(s))
                return null;

            return s;

            static string TakeAfterLastColon(string t)
            {
                t = t.TrimEnd(':').Trim();
                int lastColon = t.LastIndexOf(':');
                return (lastColon >= 0 && lastColon < t.Length - 1)
                    ? t.Substring(lastColon + 1).Trim()
                    : t;
            }

            static bool IsAlphaWord(string t)
            {
                if (t.Length == 0)
                    return false;

                for (int i = 0; i < t.Length; i++)
                {
                    char c = t[i];
                    if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                        return false;
                }

                return true;
            }
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

        public static string BuildSummaryText(IGameFamily[] gameFamilies)
        {
            long families = gameFamilies.Length;
            long games = 0;
            long parts = 0;
            long roms = 0;

            for (int i = 0; i < gameFamilies.Length; i++)
            {
                var f = gameFamilies[i];

                var g = f.GetAllGames();
                games += g.Length;

                var ps = f.GetAllGameParts(false);
                parts += ps.Length;

                for (int p = 0; p < ps.Length; p++)
                    roms += ps[p].GetChecksums().Length;
            }

            const string s = "    ";
            return $"■ Families: {families.ToString("N0") + s} ■ Entries: {games.ToString("N0") + s} ■ Parts: {parts.ToString("N0") + s} ■ ROMs: {roms.ToString("N0")}";
        }

        public static long GetTotalParts(IEnumerable<IGameFamily> gameFamilies)
        {
            long parts = 0L;
            foreach (var family in gameFamilies)
            {
                parts += family.GetAllGameParts(false).Length;
            }
            return parts;
        }

        public static string? GetGameEntityName(IGameEntity? entity)
        {
            if (entity is IGameFamily family)
                return family.GetFamilyDisplayName();

            if (entity is IGame game)
                return game.GetNameWithoutExt();

            if (entity is IGamePart part)
                return part.GetName();

            return null;
        }
        // TODO: Why are we reverse engineering the string id?
        public static DAT_GROUP_ENUM GetDatGroup(string? pointerId)
        {
            if (string.IsNullOrWhiteSpace(pointerId))
                return DAT_GROUP_ENUM.NOT_SET;

            var s = pointerId.Trim();

            // Find the very first ": "
            int first = s.IndexOf(": ", StringComparison.Ordinal);
            if (first < 0)
                return DAT_GROUP_ENUM.NOT_SET;

            int start = first + 2;
            if (start >= s.Length)
                return DAT_GROUP_ENUM.NOT_SET;

            // Take until next ':' or space (whichever comes first), or end of string.
            int colon = s.IndexOf(':', start);
            int space = s.IndexOf(' ', start);

            int end;
            if (colon < 0 && space < 0) end = s.Length;
            else if (colon < 0) end = space;
            else if (space < 0) end = colon;
            else end = Math.Min(colon, space);

            if (end <= start)
                return DAT_GROUP_ENUM.NOT_SET;

            var token = s.Substring(start, end - start).Trim();

            var result = (token.Length == 0)
                ? DAT_GROUP_ENUM.NOT_SET
                : DatinateHelper.GetEnumFromString<DAT_GROUP_ENUM>(token, DAT_GROUP_ENUM.NOT_SET);

            return result;
        }
        public static string GetGamePartNameRender(IGamePart part)
        {
            string entryName = part.GetName();
            
            // TODO: Why are we reverse engineering the string id?
            var datGroupEnum = DatinateHelper.GetDatGroup(part.GetDirectoryId() ?? string.Empty);

            if (DatinateHelper.IsMameOrMameSoftlist(datGroupEnum))
            {
                var append = string.Empty;
                bool pipe = false;

                if (!string.IsNullOrWhiteSpace(part.LaunchName)) {
                    append += part.LaunchName;
                    pipe = true;
                }

                if (!string.IsNullOrWhiteSpace(part.Tag))
                {
                    append += pipe ? " | " + part.Tag : part.Tag;
                    pipe = true;
                }

                if (!string.IsNullOrWhiteSpace(append))
                    entryName += " <" + append + ">";
            }
            else if (!string.IsNullOrWhiteSpace(part.Tag))
                entryName += " <" + part.Tag + ">";

            return entryName;
        }
        public const string LoadingHtmlBlack =
"""
<!DOCTYPE html>
<html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Black Page</title>
        <style>
            body {
                background-color: #000000; /* Sets background to black */
                margin: 0;                /* Removes default margins */
                height: 100vh;            /* Ensures full viewport height */
            }
        </style>
    </head>
<body>
</body>
</html>
""";

        public static bool IsDatFriendlyNameAcceptable(string friendlyName)
        {
            const int MaxLength = 16;

            if (friendlyName.Length > MaxLength)
                return false;

            for (int i = 0; i < friendlyName.Length; i++)
            {
                char c = friendlyName[i];

                bool isLetter =
                    (c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z');

                bool isDigit = c >= '0' && c <= '9';

                if (!isLetter && !isDigit && c != '_' && c != ' ')
                    return false;
            }

            return true;
        }

        public static void HideTabs(TabControl tabControl)
        {
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.ItemSize = new Size(0, 1);
            tabControl.SizeMode = TabSizeMode.Fixed;
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
        public static string? GetPointerSubset(string pointerId)
        {
            if (string.IsNullOrWhiteSpace(pointerId))
                return null;

            var s = pointerId.Trim();

            if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
                s = s.Substring(1, s.Length - 2).Trim();

            s = s.TrimEnd();

            if (s.Length == 0 || s[^1] != ')')
                return null;

            int depth = 0;

            for (int i = s.Length - 1; i >= 0; i--)
            {
                char c = s[i];

                if (c == ')')
                {
                    depth++;
                    continue;
                }

                if (c == '(')
                {
                    depth--;
                    if (depth == 0)
                    {
                        var inner = s.Substring(i + 1, s.Length - i - 2).Trim();
                        return inner.Length == 0 ? null : inner;
                    }
                }
            }

            return null;
        }
        public static string BuildPointerId(
            COLLECTION_SET_ENUM collectionSetEnum
            , DAT_GROUP_ENUM datGroupEnum
            , string? source
            , DatSubsetFilter? datSubsetFilter
            , string? friendlyName)
        {
            return BuildPointerIdInternal(
                collectionSetEnum
                , datGroupEnum
                , friendlyName
                , source
                , datSubsetFilter
            );
        }

        private static string BuildPointerIdInternal(
            COLLECTION_SET_ENUM? collectionSetEnum
            , DAT_GROUP_ENUM? datGroupEnum
            , string? datQuickRef
            , string? typeOrSourceStr
            , DatSubsetFilter? datSubsetFilter)
        {
            string? collectionStr = null;

            if (collectionSetEnum != null)
                collectionStr = collectionSetEnum == COLLECTION_SET_ENUM.NOT_SET
                    ? string.Empty
                    : collectionSetEnum.ToString();

            string? datGroupStr = null;
            if (datGroupEnum != null)
                datGroupStr = datGroupEnum == DAT_GROUP_ENUM.NOT_SET
                    ? string.Empty
                    : datGroupEnum.ToString();

            if (!string.IsNullOrWhiteSpace(datQuickRef))
            {
                if (!string.IsNullOrWhiteSpace(datGroupStr))
                    datGroupStr += " - " + datQuickRef;
                else
                    datGroupStr = datQuickRef;
            }

            typeOrSourceStr = string.IsNullOrWhiteSpace(typeOrSourceStr) || typeOrSourceStr == MEDIA_TYPE_ENUM.NOT_SET.ToString()
                ? string.Empty
                : typeOrSourceStr;

            string subset = string.Empty;

            if (datSubsetFilter != null)
            {
                subset = "(";
                subset += datSubsetFilter.Entry;

                if (!string.IsNullOrWhiteSpace(datSubsetFilter.Path))
                    subset += ": " + datSubsetFilter.Path;

                subset += ")";
            }

            return BuildPointerId(
                collectionStr
                , datGroupStr
                , typeOrSourceStr
                , subset
            );
        }

        private static string BuildPointerId(
            string? collection,
            string? datGroup,
            string? sourceOrType,
            string? subset)
        {
            string running = string.Empty;

            if (!string.IsNullOrWhiteSpace(collection))
                running += collection;

            if (!string.IsNullOrWhiteSpace(datGroup))
                running += running == string.Empty ? datGroup : ": " + datGroup;

            if (!string.IsNullOrWhiteSpace(sourceOrType))
                running += running == string.Empty ? sourceOrType : ": " + sourceOrType;

            if (!string.IsNullOrWhiteSpace(subset))
                running += running == string.Empty ? subset : ": " + subset;

            return running;
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
