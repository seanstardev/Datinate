using datinate.app;
using Datinate.App.WinForms.Properties;
using Datinate.Shared;
using static Datinate.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class UIHelper 
    {
        public static readonly Color POP_COLOUR = Color.FromArgb(215, 228, 242);

        public static void PopSplitter(SplitContainer splitter)
        {
            Color p1 = splitter.Panel1.BackColor;
            Color p2 = splitter.Panel2.BackColor;

            splitter.BackColor = Color.DarkGray; // splitter “line” colour

            splitter.Panel1.BackColor = p1;
            splitter.Panel2.BackColor = p2;
        }

        public static void PopButton(Button btn) 
        {
            btn.BackColor = POP_COLOUR;
        }
        public static void HideTabs(TabControl tabControl)
        {
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.ItemSize = new Size(0, 1);
            tabControl.SizeMode = TabSizeMode.Fixed;
        }
        public static bool ShowDialogYesNo(string message, string title = "Attention")
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        // TODO: Move these to a new helper?


        public static Bitmap? GetMediaIconBmp(MEDIA_TYPE_ENUM mediaTypeEnum)
        {
            var iconType = DatinateHelper.GetSupportedMediaIconType(mediaTypeEnum);

            if (!iconType.HasValue)
                return null;

            // Shared selects the icon type; WinForms supplies the corresponding asset.
            return iconType.Value switch
            {
                MEDIA_TYPE_ENUM.Advert => Resources.media_icons_Advert,
                MEDIA_TYPE_ENUM.Box => Resources.media_icons_Box,
                MEDIA_TYPE_ENUM.Info => Resources.media_icons_Web_info,
                MEDIA_TYPE_ENUM.Info_About => Resources.media_icons_Web_Info_description,
                MEDIA_TYPE_ENUM.Info_Credits => Resources.media_icons_Web_Info_credits,
                MEDIA_TYPE_ENUM.Info_Releases => Resources.media_icons_Web_Info_releases,
                MEDIA_TYPE_ENUM.Thumb => Resources.media_icons_Icon,
                MEDIA_TYPE_ENUM.Manual => Resources.media_icons_Manual,
                MEDIA_TYPE_ENUM.Media => Resources.media_icons_Media_cartridge_2,
                MEDIA_TYPE_ENUM.Other => Resources.media_icons_Other,
                MEDIA_TYPE_ENUM.Other_Map => Resources.media_icons_Map,
                MEDIA_TYPE_ENUM.Snap => Resources.media_icons_Snap,
                MEDIA_TYPE_ENUM.Soundtrack => Resources.media_icons_Soundtrack,
                MEDIA_TYPE_ENUM.Video => Resources.media_icons_Video,

                _ => throw new InvalidOperationException(
                    "No WinForms asset mapping for icon type: " + iconType.Value)
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
