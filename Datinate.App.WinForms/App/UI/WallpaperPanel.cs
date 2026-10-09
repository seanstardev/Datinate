using System.ComponentModel;

namespace datinate.app
{
    public class WallpaperPanel : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image? Wallpaper { get; set; }

        public WallpaperPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            DoubleBuffered = true;
            UpdateStyles();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Wallpaper != null)
            {
                e.Graphics.DrawImage(Wallpaper, ClientRectangle);
                return;
            }

            base.OnPaintBackground(e);
        }
    }
}
