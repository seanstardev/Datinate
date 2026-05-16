using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace datinate.app
{
    public class HighQualityPictureBox : PictureBox
    {
        private bool useRegularSettings;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseRegularSettings
        {
            get => useRegularSettings;
            set
            {
                if (useRegularSettings == value)
                    return;

                useRegularSettings = value;
                ApplyPaintingMode();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public InterpolationMode InterpolationMode { get; set; } = InterpolationMode.HighQualityBicubic;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PixelOffsetMode PixelOffsetMode { get; set; } = PixelOffsetMode.HighQuality;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CompositingQuality CompositingQuality { get; set; } = CompositingQuality.HighQuality;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SmoothingMode SmoothingMode { get; set; } = SmoothingMode.HighQuality;

        public HighQualityPictureBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            ApplyPaintingMode();
        }

        private void ApplyPaintingMode()
        {
            if (useRegularSettings)
            {
                SetStyle(ControlStyles.UserPaint, false);
                SetStyle(ControlStyles.AllPaintingInWmPaint, false);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            }
            else
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.UserPaint |
                         ControlStyles.ResizeRedraw, true);
            }

            UpdateStyles();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (useRegularSettings)
            {
                base.OnPaintBackground(e);
                return;
            }

            if (BackColor.A < 255)
            {
                if (Parent == null)
                {
                    base.OnPaintBackground(e);
                    return;
                }

                var state = e.Graphics.Save();
                e.Graphics.TranslateTransform(-Left, -Top);

                var rect = new Rectangle(Left, Top, Width, Height);
                var pea = new PaintEventArgs(e.Graphics, rect);

                InvokePaintBackground(Parent, pea);
                InvokePaint(Parent, pea);

                e.Graphics.Restore(state);
                return;
            }

            base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (useRegularSettings)
            {
                base.OnPaint(pe);
                return;
            }

            var img = Image;
            if (img is null)
            {
                base.OnPaint(pe);
                return;
            }

            pe.Graphics.CompositingQuality = CompositingQuality;
            pe.Graphics.InterpolationMode = InterpolationMode;
            pe.Graphics.PixelOffsetMode = PixelOffsetMode;
            pe.Graphics.SmoothingMode = SmoothingMode;

            using var ia = new ImageAttributes();
            ia.SetWrapMode(WrapMode.TileFlipXY);

            var dest = GetDestRect(img);
            pe.Graphics.DrawImage(img, dest, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
        }

        private Rectangle GetDestRect(Image img)
        {
            switch (SizeMode)
            {
                case PictureBoxSizeMode.Normal:
                    return new Rectangle(0, 0, img.Width, img.Height);

                case PictureBoxSizeMode.StretchImage:
                    return ClientRectangle;

                case PictureBoxSizeMode.CenterImage:
                    {
                        int x = (ClientSize.Width - img.Width) / 2;
                        int y = (ClientSize.Height - img.Height) / 2;
                        return new Rectangle(x, y, img.Width, img.Height);
                    }

                case PictureBoxSizeMode.Zoom:
                    {
                        var cw = ClientSize.Width;
                        var ch = ClientSize.Height;

                        if (cw <= 0 || ch <= 0)
                            return Rectangle.Empty;

                        float rx = (float)cw / img.Width;
                        float ry = (float)ch / img.Height;
                        float r = Math.Min(rx, ry);

                        int w = (int)(img.Width * r);
                        int h = (int)(img.Height * r);

                        int x = (cw - w) / 2;
                        int y = (ch - h) / 2;

                        return new Rectangle(x, y, w, h);
                    }

                case PictureBoxSizeMode.AutoSize:
                    return new Rectangle(0, 0, img.Width, img.Height);

                default:
                    return ClientRectangle;
            }
        }
    }
}
