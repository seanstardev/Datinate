using datinate.app;
using Datinate.Properties;
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
