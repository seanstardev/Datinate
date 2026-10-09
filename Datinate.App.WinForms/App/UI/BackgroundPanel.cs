using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    public sealed class BackgroundPanel : Panel
    {
        private static readonly Color DefaultCenterColor = Color.White;
        private static readonly Color DefaultCornerColor = Color.FromArgb(200, 200, 200);

        private Color centerColor = DefaultCenterColor;
        private Color cornerColor = DefaultCornerColor;
        private bool highQuality = true;

        private Bitmap? cached;
        private Size cachedSize;
        private Color cachedCenter;
        private Color cachedCorner;
        private bool cachedHQ;

        public BackgroundPanel()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color CenterColor
        {
            get => centerColor;
            set
            {
                if (centerColor == value)
                    return;

                centerColor = value;
                InvalidateCache();
            }
        }
        public void ResetCenterColor() => CenterColor = DefaultCenterColor;
        public bool ShouldSerializeCenterColor() => centerColor != DefaultCenterColor;

        [Browsable(true)]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color CornerColor
        {
            get => cornerColor;
            set
            {
                if (cornerColor == value)
                    return;

                cornerColor = value;
                InvalidateCache();
            }
        }
        public void ResetCornerColor() => CornerColor = DefaultCornerColor;
        public bool ShouldSerializeCornerColor() => cornerColor != DefaultCornerColor;

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool HighQuality
        {
            get => highQuality;
            set
            {
                if (highQuality == value)
                    return;

                highQuality = value;
                InvalidateCache();
            }
        }
        public void ResetHighQuality() => HighQuality = true;
        public bool ShouldSerializeHighQuality() => highQuality != true;

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            InvalidateCache();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            InvalidateCache();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                cached?.Dispose();

            cached = null;
            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var r = ClientRectangle;
            if (r.Width <= 0 || r.Height <= 0)
                return;

            EnsureCache(r.Size);

            if (cached != null)
                e.Graphics.DrawImageUnscaled(cached, 0, 0);
        }

        private void InvalidateCache()
        {
            cached?.Dispose();
            cached = null;
            Invalidate();
        }

        private void EnsureCache(Size size)
        {
            if (cached != null &&
                cachedSize == size &&
                cachedCenter == centerColor &&
                cachedCorner == cornerColor &&
                cachedHQ == highQuality)
                return;

            cached?.Dispose();

            cachedSize = size;
            cachedCenter = centerColor;
            cachedCorner = cornerColor;
            cachedHQ = highQuality;

            var bmp = new Bitmap(size.Width, size.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                if (highQuality)
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                }
                else
                {
                    g.SmoothingMode = SmoothingMode.None;
                    g.PixelOffsetMode = PixelOffsetMode.None;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                }

                g.Clear(cornerColor);

                float cx = size.Width / 2f;
                float cy = size.Height / 2f;

                float rx = size.Width / 2f;
                float ry = size.Height / 2f;
                float r = (float)Math.Sqrt((rx * rx) + (ry * ry));

                var ellipseRect = new RectangleF(cx - r, cy - r, 2f * r, 2f * r);

                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(ellipseRect);

                    using (var pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterColor = centerColor;
                        pgb.SurroundColors = new[] { cornerColor };
                        pgb.CenterPoint = new PointF(cx, cy);

                        g.FillRectangle(pgb, new Rectangle(0, 0, size.Width, size.Height));
                    }
                }
            }

            cached = bmp;
        }
    }
}
