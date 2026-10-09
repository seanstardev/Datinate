using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    public partial class ContentPathsHeader : UserControl
    {
        private static readonly Color SectionBorderColor = Color.FromArgb(196, 205, 216);
        private static readonly Color MutedText = Color.FromArgb(88, 96, 105);
        private bool columnHeadersVisible = true;

        private int contentMaxWidthPx = 0;

        private ColumnStyle[]? baseSpec;

        private Control? hookedParent;
        private bool syncingWidth;

        public ContentPathsHeader()
        {
            InitializeComponent();
            DoubleBuffered = true;

            Dock = DockStyle.Top;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            titleLbl.ForeColor = Color.Black;
            nameHdrLbl.ForeColor = MutedText;
            pathHdrLbl.ForeColor = MutedText;
            hideHdrLbl.ForeColor = MutedText;
            itemsHdrLbl.ForeColor = MutedText;

            infoPanel.Paint += InfoPanel_Paint;

            InfoText = string.Empty;
            ColumnHeadersVisible = true;
            ShowMoveButtonsSpacerColumn = true;
            ShowHideColumn = true;
            ShowItemsColumn = true;

            ApplyAll();
        }

        [Category("Content")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string TitleText
        {
            get => titleLbl.Text;
            set
            {
                var text = value ?? string.Empty;
                if (titleLbl.Text == text)
                    return;

                titleLbl.Text = text;
                Invalidate();
            }
        }

        [Category("Content")]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string InfoText
        {
            get => infoLbl.Text;
            set
            {
                var text = value ?? string.Empty;
                if (infoLbl.Text == text)
                    return;

                infoLbl.Text = text;
                ApplyVisibilityLayout();
                if (infoPanel.Visible)
                {
                    infoPanel.Invalidate();
                    infoPanel.Update();
                }
                Invalidate(true);

                if (IsHandleCreated)
                    Update();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool InfoVisible
        {
            get => !string.IsNullOrWhiteSpace(infoLbl.Text);
            set
            {
                if (!value)
                    InfoText = string.Empty;
                else
                    ApplyVisibilityLayout();
            }
        }

        [Category("Layout")]
        [DefaultValue(true)]
        public bool ColumnHeadersVisible
        {
            get => columnHeadersVisible;
            set
            {
                if (columnHeadersVisible == value)
                    return;

                columnHeadersVisible = value;
                columnHeaderPanel.Visible = value;

                ApplyVisibilityLayout();
            }
        }

        [Category("Layout")]
        [DefaultValue(true)]
        public bool ShowMoveButtonsSpacerColumn { get; set; } = true;

        [Category("Layout")]
        [DefaultValue(true)]
        public bool ShowHideColumn { get; set; } = true;

        [Category("Layout")]
        [DefaultValue(true)]
        public bool ShowItemsColumn { get; set; } = true;

        public void SetColumnHeaderSpec(ColumnStyle[] spec)
        {
            baseSpec = spec;
            ApplyAll();
        }

        public void ApplyAll()
        {
            EnsureBaseSpecFromRowIfUnset();
            RebuildColumnHeader();
            ApplyVisibilityLayout();
            ApplyWidthCap();
            Invalidate();
        }

        private void EnsureBaseSpecFromRowIfUnset()
        {
            if (baseSpec is not null)
                return;

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            var snap = ContentPathRow.GetDefaultLayoutColumnStyleSnapshotStatic();
            if (snap.Length != 5)
                return;

            baseSpec = new[]
            {
                new ColumnStyle(snap[0].SizeType, snap[0].Width),
                new ColumnStyle(snap[1].SizeType, snap[1].Width),
                new ColumnStyle(snap[2].SizeType, snap[2].Width),
                new ColumnStyle(snap[3].SizeType, snap[3].Width),
                new ColumnStyle(snap[4].SizeType, snap[4].Width),
            };
        }

        private void ApplyVisibilityLayout()
        {
            var infoShouldShow = !string.IsNullOrWhiteSpace(infoLbl.Text);
            var any = infoShouldShow || columnHeadersVisible;

            if (infoPanel.Visible != infoShouldShow)
                infoPanel.Visible = infoShouldShow;

            if (bodyOuterPanel.Visible != any)
                bodyOuterPanel.Visible = any;

            if (columnHeaderPanel.Visible != columnHeadersVisible)
                columnHeaderPanel.Visible = columnHeadersVisible;

            if (infoPanel.Visible)
            {
                infoPanel.Invalidate();
                infoPanel.Update();
            }

            capPanel.PerformLayout();
            bodyOuterPanel.PerformLayout();
            PerformLayout();
            Invalidate(true);
        }

        private void SyncWidthToParent()
        {
            if (Parent is null)
                return;

            var layoutW = Parent is ScrollableControl sc ? sc.DisplayRectangle.Width : Parent.ClientSize.Width;
            var targetW = System.Math.Max(1, layoutW - Margin.Horizontal);

            if (Width == targetW)
                return;

            syncingWidth = true;
            try
            {
                Width = targetW;
            }
            finally
            {
                syncingWidth = false;
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var s = base.GetPreferredSize(proposedSize);

            if (Parent is null)
                return s;

            var layoutW = Parent is ScrollableControl sc ? sc.DisplayRectangle.Width : Parent.ClientSize.Width;
            if (layoutW > 1)
                s.Width = System.Math.Max(1, layoutW - Margin.Horizontal);

            return s;
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            base.OnSizeChanged(e);

            if (!syncingWidth)
                SyncWidthToParent();

            ApplyWidthCap();
            Invalidate();
        }

        private void InfoPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (!infoPanel.Visible)
                return;

            var r = infoPanel.ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;

            using var pen = new Pen(SectionBorderColor, 1);
            pen.DashStyle = DashStyle.Solid;

            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.DrawRectangle(pen, r);
        }

        private void ApplyWidthCap()
        {
            var baseLeft = 12;
            var baseTop = 10;
            var baseRight = 12;
            var baseBottom = 12;

            if (contentMaxWidthPx <= 0 || capPanel.Width <= 1)
            {
                if (capPanel.Padding.Left != baseLeft || capPanel.Padding.Top != baseTop || capPanel.Padding.Right != baseRight || capPanel.Padding.Bottom != baseBottom)
                    capPanel.Padding = new Padding(baseLeft, baseTop, baseRight, baseBottom);

                return;
            }

            var available = capPanel.Width - baseLeft - baseRight;
            var extraRight = available > contentMaxWidthPx ? (available - contentMaxWidthPx) : 0;
            var newRight = baseRight + extraRight;

            if (capPanel.Padding.Left != baseLeft || capPanel.Padding.Top != baseTop || capPanel.Padding.Right != newRight || capPanel.Padding.Bottom != baseBottom)
                capPanel.Padding = new Padding(baseLeft, baseTop, newRight, baseBottom);
        }

        private void RebuildColumnHeader()
        {
            EnsureBaseSpecFromRowIfUnset();

            var spec = baseSpec;

            columnLayout.SuspendLayout();
            try
            {
                columnLayout.Controls.Clear();
                columnLayout.ColumnStyles.Clear();

                var defaultSpec = new[]
                {
                    new ColumnStyle(SizeType.Absolute, 240F),
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle(SizeType.Absolute, 34F),
                    new ColumnStyle(SizeType.Absolute, 100F),
                    new ColumnStyle(SizeType.Absolute, 50F),
                };

                if (spec == null || spec.Length < 3)
                    spec = defaultSpec;

                var cols = 0;

                if (ShowMoveButtonsSpacerColumn)
                {
                    cols++;
                    columnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
                }

                for (int i = 0; i < spec.Length; i++)
                {
                    var c = spec[i];
                    var w = c.Width;

                    if (i == 3 && !ShowItemsColumn) w = 0F;
                    if (i == 4 && !ShowHideColumn) w = 0F;

                    cols++;
                    columnLayout.ColumnStyles.Add(new ColumnStyle(c.SizeType, w));
                }

                columnLayout.ColumnCount = cols;
                columnLayout.RowCount = 1;
                columnLayout.RowStyles.Clear();
                columnLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                var col = 0;

                if (ShowMoveButtonsSpacerColumn)
                    columnLayout.Controls.Add(moveHdrLbl, col++, 0);

                columnLayout.Controls.Add(nameHdrLbl, col++, 0);
                columnLayout.Controls.Add(pathHdrLbl, col++, 0);
                columnLayout.Controls.Add(browseHdrLbl, col++, 0);

                if (spec.Length > 3)
                {
                    itemsHdrLbl.Visible = ShowItemsColumn && columnLayout.ColumnStyles[col].Width > 0.01f;
                    columnLayout.Controls.Add(itemsHdrLbl, col++, 0);
                }
                else itemsHdrLbl.Visible = false;

                if (spec.Length > 4)
                {
                    hideHdrLbl.Visible = ShowHideColumn && columnLayout.ColumnStyles[col].Width > 0.01f;
                    columnLayout.Controls.Add(hideHdrLbl, col++, 0);
                }
                else hideHdrLbl.Visible = false;
            }
            finally
            {
                columnLayout.ResumeLayout(true);
            }
        }

        protected override void OnParentChanged(System.EventArgs e)
        {
            base.OnParentChanged(e);

            if (hookedParent is not null)
                hookedParent.SizeChanged -= Parent_SizeChanged;

            hookedParent = Parent;

            if (hookedParent is not null)
                hookedParent.SizeChanged += Parent_SizeChanged;

            SyncWidthToParent();
            ApplyWidthCap();
        }

        private void Parent_SizeChanged(object? sender, System.EventArgs e)
        {
            SyncWidthToParent();
            ApplyWidthCap();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            var r = ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;

            using var p = new Pen(SectionBorderColor);
            g.DrawRectangle(p, r);

            var y = headerPanel.Bottom;
            g.DrawLine(p, 0, y, Width - 1, y);
        }
    }
}
