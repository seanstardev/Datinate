using System.ComponentModel;
using System.Drawing.Drawing2D;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public sealed class AssignmentBanner : PictureBox
    {
        public const int DefaultBandHeightPx = 44;
        public const int DefaultBandMinHeightPx = 28;
        public const int DefaultBandMaxHeightPx = 70;

        public const float DefaultBandInsetXRatio = 0.02f;
        public const float DefaultCornerRadiusRatio = 0.35f;
        public const float DefaultBandBorderWidthRatio = 0.06f;

        public const float DefaultIconSizeToBandHeightRatio = 0.86f;
        public const float DefaultIconBorderWidthRatio = 0.10f;

        public const float DefaultTickStrokeWidthRatio = 0.13f;
        public const float DefaultTickP1X = 0.22f;
        public const float DefaultTickP1Y = 0.58f;
        public const float DefaultTickP2X = 0.44f;
        public const float DefaultTickP2Y = 0.78f;
        public const float DefaultTickP3X = 0.78f;
        public const float DefaultTickP3Y = 0.30f;

        public const float DefaultNoEntrySlashWidthRatio = 0.14f;
        public const float DefaultNoEntryInnerPadRatio = 0.18f;

        public static readonly Color DefaultBandFill = Color.FromArgb(245, 255, 255, 255);
        public static readonly Color DefaultBandBorder = Color.FromArgb(120, 40, 40, 40);

        public static readonly Color DefaultAssignedCircleFill = Color.FromArgb(255, 121, 196, 121);
        public static readonly Color DefaultAssignedCircleBorder = Color.FromArgb(255, 86, 160, 86);
        public static readonly Color DefaultAssignedMark = Color.FromArgb(255, 255, 255, 255);

        public static readonly Color DefaultNotFoundCircleFill = Color.FromArgb(255, 210, 210, 210);
        public static readonly Color DefaultNotFoundCircleBorder = Color.FromArgb(255, 145, 145, 145);
        public static readonly Color DefaultNotFoundMark = Color.FromArgb(255, 120, 120, 120);

        private MEDIA_ASSIGNMENT_ENUM assignmentState = MEDIA_ASSIGNMENT_ENUM.None;

        private Control? target;
        private bool hitTestTransparent = true;

        public AssignmentBanner()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            TabStop = false;
            SizeMode = PictureBoxSizeMode.Normal;

            BackColor = Color.Transparent;
            Visible = false;
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(typeof(MEDIA_ASSIGNMENT_ENUM), nameof(MEDIA_ASSIGNMENT_ENUM.None))]
        public MEDIA_ASSIGNMENT_ENUM AssignmentState
        {
            get => assignmentState;
            set
            {
                if (assignmentState == value)
                    return;

                assignmentState = value;
                Visible = assignmentState != MEDIA_ASSIGNMENT_ENUM.None;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HitTestTransparent
        {
            get => hitTestTransparent;
            set => hitTestTransparent = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BandHeightPx96 { get; set; } = DefaultBandHeightPx;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BandMinHeightPx96 { get; set; } = DefaultBandMinHeightPx;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BandMaxHeightPx96 { get; set; } = DefaultBandMaxHeightPx;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float BandInsetXRatio { get; set; } = DefaultBandInsetXRatio;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float CornerRadiusRatio { get; set; } = DefaultCornerRadiusRatio;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float BandBorderWidthRatio { get; set; } = DefaultBandBorderWidthRatio;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float IconSizeToBandHeightRatio { get; set; } = DefaultIconSizeToBandHeightRatio;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float IconBorderWidthRatio { get; set; } = DefaultIconBorderWidthRatio;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TickStrokeWidthRatio { get; set; } = DefaultTickStrokeWidthRatio;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TickP1X { get; set; } = DefaultTickP1X;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TickP1Y { get; set; } = DefaultTickP1Y;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TickP2X { get; set; } = DefaultTickP2X;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TickP2Y { get; set; } = DefaultTickP2Y;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TickP3X { get; set; } = DefaultTickP3X;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TickP3Y { get; set; } = DefaultTickP3Y;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float NoEntrySlashWidthRatio { get; set; } = DefaultNoEntrySlashWidthRatio;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float NoEntryInnerPadRatio { get; set; } = DefaultNoEntryInnerPadRatio;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BandFill { get; set; } = DefaultBandFill;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BandBorder { get; set; } = DefaultBandBorder;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color AssignedCircleFill { get; set; } = DefaultAssignedCircleFill;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color AssignedCircleBorder { get; set; } = DefaultAssignedCircleBorder;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color AssignedMark { get; set; } = DefaultAssignedMark;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color NotFoundCircleFill { get; set; } = DefaultNotFoundCircleFill;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color NotFoundCircleBorder { get; set; } = DefaultNotFoundCircleBorder;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color NotFoundMark { get; set; } = DefaultNotFoundMark;

        public void AttachTo(Control targetControl)
        {
            Detach();

            target = targetControl ?? throw new ArgumentNullException(nameof(targetControl));

            if (target.Parent == null)
                throw new InvalidOperationException("Target must have a Parent before attaching.");

            Parent = target.Parent;
            Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Left = target.Left;
            Width = target.Width;

            UpdateVerticalPlacement();
            BringToFront();

            target.LocationChanged += TargetChanged;
            target.SizeChanged += TargetChanged;
            target.VisibleChanged += TargetChanged;
            target.ParentChanged += TargetParentChanged;

            if (Parent != null)
                Parent.ControlAdded += ParentControlAdded;
        }

        public void Detach()
        {
            if (target == null)
                return;

            target.LocationChanged -= TargetChanged;
            target.SizeChanged -= TargetChanged;
            target.VisibleChanged -= TargetChanged;
            target.ParentChanged -= TargetParentChanged;

            if (Parent != null)
                Parent.ControlAdded -= ParentControlAdded;

            target = null;
        }

        private void ParentControlAdded(object? sender, ControlEventArgs e)
        {
            BringToFront();
        }

        private void TargetParentChanged(object? sender, EventArgs e)
        {
            if (target == null)
                return;

            if (Parent != null)
                Parent.ControlAdded -= ParentControlAdded;

            if (target.Parent == null)
                return;

            Parent = target.Parent;
            Parent.ControlAdded += ParentControlAdded;

            TargetChanged(null, EventArgs.Empty);
        }

        private void TargetChanged(object? sender, EventArgs e)
        {
            if (target == null)
                return;

            Visible = target.Visible && assignmentState != MEDIA_ASSIGNMENT_ENUM.None;

            Left = target.Left;
            Width = target.Width;

            UpdateVerticalPlacement();
            BringToFront();
            Invalidate();
        }

        private void UpdateVerticalPlacement()
        {
            if (target == null)
                return;

            float dpi = DeviceDpi > 0 ? DeviceDpi / 96f : 1f;

            int h = (int)Math.Round(BandHeightPx96 * dpi);
            int minH = (int)Math.Round(BandMinHeightPx96 * dpi);
            int maxH = (int)Math.Round(BandMaxHeightPx96 * dpi);

            h = Math.Max(minH, Math.Min(maxH, h));
            Height = h;

            Top = target.Top + (target.Height - Height) / 2;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (assignmentState == MEDIA_ASSIGNMENT_ENUM.None)
                return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float dpi = DeviceDpi > 0 ? DeviceDpi / 96f : 1f;
            float minStroke = Math.Max(1f, 1f * dpi);

            var cr = new RectangleF(0, 0, ClientSize.Width, ClientSize.Height);

            float insetX = Math.Max(0f, cr.Width * BandInsetXRatio);
            var bandRect = new RectangleF(cr.Left + insetX, cr.Top, Math.Max(1f, cr.Width - insetX * 2f), cr.Height);

            float radius = Math.Max(2f * dpi, bandRect.Height * CornerRadiusRatio);
            float borderW = Math.Max(minStroke, bandRect.Height * BandBorderWidthRatio);

            using (var path = CreateRoundedRectPath(bandRect, radius))
            using (var fill = new SolidBrush(BandFill))
            using (var pen = new Pen(BandBorder, borderW) { Alignment = PenAlignment.Inset })
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }

            float iconD = Math.Max(10f * dpi, bandRect.Height * IconSizeToBandHeightRatio);
            var iconRect = new RectangleF(
                bandRect.Left + (bandRect.Width - iconD) * 0.5f,
                bandRect.Top + (bandRect.Height - iconD) * 0.5f,
                iconD,
                iconD);

            if (assignmentState == MEDIA_ASSIGNMENT_ENUM.Assigned)
                DrawTick(g, iconRect, minStroke);
            else
                DrawNoEntry(g, iconRect, minStroke);
        }

        private void DrawTick(Graphics g, RectangleF rect, float minStroke)
        {
            float borderW = Math.Max(minStroke, rect.Width * IconBorderWidthRatio);
            float tickW = Math.Max(minStroke, rect.Width * TickStrokeWidthRatio);
            var cap = tickW <= (1.25f * (DeviceDpi > 0 ? DeviceDpi / 96f : 1f)) ? LineCap.Square : LineCap.Round;

            using (var fill = new SolidBrush(AssignedCircleFill))
                g.FillEllipse(fill, rect);

            using (var border = new Pen(AssignedCircleBorder, borderW) { Alignment = PenAlignment.Inset })
                g.DrawEllipse(border, rect);

            float x = rect.Left;
            float y = rect.Top;
            float w = rect.Width;
            float h = rect.Height;

            var p1 = new PointF(x + w * TickP1X, y + h * TickP1Y);
            var p2 = new PointF(x + w * TickP2X, y + h * TickP2Y);
            var p3 = new PointF(x + w * TickP3X, y + h * TickP3Y);

            using (var pen = new Pen(AssignedMark, tickW)
            {
                StartCap = cap,
                EndCap = cap,
                LineJoin = LineJoin.Round
            })
            {
                g.DrawLines(pen, new[] { p1, p2, p3 });
            }
        }

        private void DrawNoEntry(Graphics g, RectangleF rect, float minStroke)
        {
            float borderW = Math.Max(minStroke, rect.Width * IconBorderWidthRatio);
            float slashW = Math.Max(minStroke, rect.Width * NoEntrySlashWidthRatio);
            float pad = Math.Max(minStroke, rect.Width * NoEntryInnerPadRatio);

            using (var fill = new SolidBrush(NotFoundCircleFill))
                g.FillEllipse(fill, rect);

            using (var border = new Pen(NotFoundCircleBorder, borderW) { Alignment = PenAlignment.Inset })
                g.DrawEllipse(border, rect);

            var p1 = new PointF(rect.Left + pad, rect.Bottom - pad);
            var p2 = new PointF(rect.Right - pad, rect.Top + pad);

            using (var pen = new Pen(NotFoundMark, slashW)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            })
            {
                g.DrawLine(pen, p1, p2);
            }
        }

        private static GraphicsPath CreateRoundedRectPath(RectangleF rect, float radius)
        {
            float r = Math.Max(0f, radius);
            float d = Math.Min(Math.Min(rect.Width, rect.Height), r * 2f);

            var path = new GraphicsPath();
            if (d <= 0.01f)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            var arc = new RectangleF(rect.X, rect.Y, d, d);

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = -1;

            if (hitTestTransparent && m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
                return;
            }

            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Detach();

            base.Dispose(disposing);
        }
    }
}
