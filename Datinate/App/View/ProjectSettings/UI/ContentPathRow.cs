using com.RADIO.Datinate.RMVC.Shared;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public sealed partial class ContentPathRow : UserControl
    {
        public const int AuxRowHeightPx = 48;
        private const int AuxMoveButtonsColumnWidthPx = 60;

        public event Action<ContentPathRow>? Changed;
        public event Action<ContentPathRow>? MoveUpRequested;
        public event Action<ContentPathRow>? MoveDownRequested;
        public event Action<ContentPathRow>? MoveTopRequested;
        public event Action<ContentPathRow>? MoveBottomRequested;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DAT_GROUP_ENUM? DatGroupEnum { get; private set; } = null;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? PointerId { get; private set; } = null;


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public (SizeType SizeType, float Width)[] GetLayoutColumnStyleSnapshot()
        {
            var n = layout.ColumnStyles.Count;
            var arr = new (SizeType SizeType, float Width)[n];

            for (int i = 0; i < n; i++)
            {
                var cs = layout.ColumnStyles[i];
                arr[i] = (cs.SizeType, cs.Width);
            }

            return arr;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? Path
        {
            get => pathTextBox.Text;
            set
            {
                _suppressEvents = true;
                try { pathTextBox.Text = value ?? string.Empty; }
                finally { _suppressEvents = false; }
                RaiseChanged();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Hide_
        {
            get => hideCheckBox.Checked;
            set
            {
                if (!hideCheckBox.Enabled) return;

                _suppressEvents = true;
                try { hideCheckBox.Checked = value; }
                finally { _suppressEvents = false; }
                RaiseChanged();
            }
        }

        private static readonly Color StripeBack = Color.FromArgb(250, 252, 255);
        private static (SizeType SizeType, float Width)[]? defaultLayoutSpec;

        private readonly ArrowButton upBtn;
        private readonly ArrowButton downBtn;
        private readonly ArrowButton topBtn;
        private readonly ArrowButton bottomBtn;
        private readonly TableLayoutPanel leftBtnPanel;

        private DatGrouperProjectEntry? sourceModel = null;

        private bool _suppressEvents;

        private bool limitInteraction = false;
        private bool interactionSnapshotValid;
        private bool snapHideEnabled;
        private bool snapUpEnabled;
        private bool snapDownEnabled;
        private bool snapTopEnabled;
        private bool snapBottomEnabled;

        private bool enforcingInteractionLock;

        private bool snapHideEnabledBusy;

        public ContentPathRow()
        {
            InitializeComponent();
            CaptureDefaultLayoutSpecFromThisInstance();

            DoubleBuffered = true;

            var baseFont = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
            Font = baseFont;

            pathTextBox.Font = baseFont;

            DoubleBuffered = true;

            AutoSize = false;
            Height = AuxRowHeightPx;
            MinimumSize = new Size(0, AuxRowHeightPx);
            MaximumSize = new Size(int.MaxValue, AuxRowHeightPx);
            Margin = new Padding(0);
            Padding = new Padding(0);

            upBtn = new ArrowButton(ArrowButton.Dir.Up);
            downBtn = new ArrowButton(ArrowButton.Dir.Down);
            topBtn = new ArrowButton(ArrowButton.Dir.Top);
            bottomBtn = new ArrowButton(ArrowButton.Dir.Bottom);

            snapUpEnabled = upBtn.Enabled;
            snapDownEnabled = downBtn.Enabled;
            snapTopEnabled = topBtn.Enabled;
            snapBottomEnabled = bottomBtn.Enabled;

            leftBtnPanel = new TableLayoutPanel
            {
                Width = AuxMoveButtonsColumnWidthPx,
                Dock = DockStyle.Left,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(4, 3, 2, 3),
                Margin = new Padding(0),
                BackColor = Color.Transparent,
                MinimumSize = new Size(AuxMoveButtonsColumnWidthPx, AuxRowHeightPx),
                MaximumSize = new Size(AuxMoveButtonsColumnWidthPx, AuxRowHeightPx)
            };

            leftBtnPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26F));
            leftBtnPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26F));
            leftBtnPanel.RowStyles.Clear();
            leftBtnPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            leftBtnPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            upBtn.Dock = DockStyle.Fill;
            downBtn.Dock = DockStyle.Fill;
            topBtn.Dock = DockStyle.Fill;
            bottomBtn.Dock = DockStyle.Fill;

            leftBtnPanel.Controls.Add(upBtn, 0, 0);
            leftBtnPanel.Controls.Add(downBtn, 0, 1);
            leftBtnPanel.Controls.Add(topBtn, 1, 0);
            leftBtnPanel.Controls.Add(bottomBtn, 1, 1);

            pathTextBox.TextChanged += (_, __) => { if (!_suppressEvents) RaiseChanged(); };
            hideCheckBox.CheckedChanged += (_, __) => { if (!_suppressEvents) RaiseChanged(); };
            
            Controls.Add(leftBtnPanel);

            upBtn.Click += (_, _) => MoveUpRequested?.Invoke(this);
            downBtn.Click += (_, _) => MoveDownRequested?.Invoke(this);
            topBtn.Click += (_, _) => MoveTopRequested?.Invoke(this);
            bottomBtn.Click += (_, _) => MoveBottomRequested?.Invoke(this);

            browseBtn.Click += (_, __) => BrowseForFolder();

            Cursor = Cursors.NoMove2D;

            ApplyBackColorToChildren();
            WireInteractionLockEnforcement();
            ApplyInteractionLock();
            HookBusyLifetime();
        }
        public void SetRow(DatGrouperProjectEntry dto, bool limitInteraction)
        {
            this.limitInteraction = limitInteraction;
            ApplyInteractionLock();

            sourceModel = dto;

            DatGroupEnum = dto.DatGroupEnum;
            PointerId = dto.ID;

            _suppressEvents = true;

            try
            {
                pathTextBox.Text = dto.ContentPath ?? string.Empty;
                hideCheckBox.Checked = dto.HideInUi;

                mediaItemUI.DatChipKey = dto.ID ?? string.Empty;

                var label = dto.InternalDescriptor ?? string.Empty;
                mediaItemUI.MediaDescription = label;

                datLabel.Text = dto.DatFullpath;

                if (dto.CollectionSetEnum != COLLECTION_SET_ENUM.Software)
                    mediaItemUI.IconImageKey = dto.InternalDescriptor ?? string.Empty;
                else
                    mediaItemUI.HideIcon = true;

                filesFoldersLabel.Text = string.Empty;
            }
            finally
            {
                _suppressEvents = false;
            }

            RaiseChanged();
            Invalidate(true);

            var contentPath = dto.ContentPath;

            if (IsDisposed || Disposing)
                return;

            if (IsHandleCreated)
            {
                BeginInvoke((Action)(() =>
                {
                    if (IsDisposed || Disposing)
                        return;

                    RequestFileFolderSummaryAsync(contentPath);
                }));
            }
            else
            {
                EventHandler? onHandleCreated = null;
                onHandleCreated = (_, __) =>
                {
                    HandleCreated -= onHandleCreated;

                    if (IsDisposed || Disposing)
                        return;

                    BeginInvoke((Action)(() =>
                    {
                        if (IsDisposed || Disposing)
                            return;

                        RequestFileFolderSummaryAsync(contentPath);
                    }));
                };

                HandleCreated += onHandleCreated;
            }
        }
        public DatGrouperProjectEntry GetRow()
        {
            if (sourceModel == null)
                return DatGrouperProjectEntry.Empty;

            return new DatGrouperProjectEntry(
                sourceModel.CollectionSetEnum,
                sourceModel.DatFullpath,
                sourceModel.ID,
                sourceModel.ExpressionsXmlFullpath,
                sourceModel.DatGroupEnum,
                sourceModel.FriendlyName,
                sourceModel.DatSubsetFilter,
                sourceModel.InternalDescriptor,
                sourceModel.Comment,
                pathTextBox.Text ?? string.Empty,
                hideCheckBox.Checked);
        }

        public void UpdateFilesFoldersCount()
        {
            RequestFileFolderSummaryAsync(Path);
        }

        public void UpdateFileFolderCounts()
        {
            RequestFileFolderSummaryAsync(pathTextBox.Text);
        }
        public void SetRowHeightPx(int heightPx)
        {
            if (heightPx < 24)
                heightPx = 24;

            if (Height != heightPx)
                Height = heightPx;

            if (MinimumSize.Height != heightPx)
                MinimumSize = new Size(0, heightPx);

            if (MaximumSize.Height != heightPx)
                MaximumSize = new Size(int.MaxValue, heightPx);

            if (layout.RowStyles.Count > 0)
            {
                var rs = layout.RowStyles[0];
                if (rs.SizeType != SizeType.Absolute)
                    rs.SizeType = SizeType.Absolute;

                if (Math.Abs(rs.Height - heightPx) > 0.01f)
                    rs.Height = heightPx;
            }
        }

        public static (SizeType SizeType, float Width)[] GetDefaultLayoutColumnStyleSnapshotStatic()
        {
            if (defaultLayoutSpec is not null)
                return defaultLayoutSpec;

            using var tmp = new ContentPathRow();
            tmp.CaptureDefaultLayoutSpecFromThisInstance();
            return defaultLayoutSpec ?? Array.Empty<(SizeType, float)>();
        }

        public void SetStripe(bool on)
        {
            var bg = on ? StripeBack : Color.White;
            BackColor = bg;
            BackColor = bg;
            leftBtnPanel.BackColor = bg;
        }
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            ApplyBackColorToChildren();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }
        private void BrowseForFolder()
        {
            var initial = pathTextBox.Text ?? string.Empty;

            var revertColour = BackColor;
            BackColor = Color.FromArgb(255, 255, 192);

            using var dlg = new FolderBrowserDialog
            {
                UseDescriptionForTitle = true,
                Description = "Select a content folder",
                SelectedPath = initial,
                ShowNewFolderButton = true
            };

            var owner = FindForm();

            var result = owner is null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
            if (result != DialogResult.OK)
            {
                BackColor = revertColour;
                return;
            }
            BackColor = revertColour;

            var selected = dlg.SelectedPath ?? string.Empty;

            if (string.Equals(selected, initial, StringComparison.OrdinalIgnoreCase))
                return;

            _suppressEvents = true;
            try
            {
                Path = selected;
            }
            finally
            {
                _suppressEvents = false;
            }

            RequestFileFolderSummaryAsync(Path);
            RaiseChanged();
        }
        private void HookBusyLifetime()
        {
            EnsureBusyOverlay();
            Disposed += (_, __) => CancelCountWork();
        }
        private void ApplyBackColorToChildren()
        {
            layout.BackColor = BackColor;
            hideCheckBox.BackColor = BackColor;
            pathTextBox.BackColor = Color.White;
        }
        private void WireInteractionLockEnforcement()
        {
            hideCheckBox.EnabledChanged += InteractionLock_EnabledChanged;

            upBtn.EnabledChanged += InteractionLock_EnabledChanged;
            downBtn.EnabledChanged += InteractionLock_EnabledChanged;
            topBtn.EnabledChanged += InteractionLock_EnabledChanged;
            bottomBtn.EnabledChanged += InteractionLock_EnabledChanged;
        }

        private void InteractionLock_EnabledChanged(object? sender, EventArgs e)
        {
            if (!limitInteraction)
                return;

            if (enforcingInteractionLock)
                return;

            if (sender is Control c && c.Enabled)
                ApplyInteractionLock();
        }
        public void SetMoveEnabled(bool up, bool down, bool top, bool bottom)
        {
            snapUpEnabled = up;
            snapDownEnabled = down;
            snapTopEnabled = top;
            snapBottomEnabled = bottom;

            ApplyInteractionLock();
        }

        public void CancelButtonPresses()
        {
            upBtn.CancelPress();
            downBtn.CancelPress();
            topBtn.CancelPress();
            bottomBtn.CancelPress();
        }

        private void ApplyMoveEnabledUnlocked()
        {
            var up = snapUpEnabled;
            var down = snapDownEnabled;
            var top = snapTopEnabled;
            var bottom = snapBottomEnabled;

            GetEdgeFlags(out var isFirst, out var isLast);

            if (isFirst)
            {
                up = false;
                top = false;
            }

            if (isLast)
            {
                down = false;
                bottom = false;
            }

            upBtn.Enabled = up;
            downBtn.Enabled = down;
            topBtn.Enabled = top;
            bottomBtn.Enabled = bottom;
        }

        private void GetEdgeFlags(out bool isFirst, out bool isLast)
        {
            isFirst = false;
            isLast = false;

            var parent = Parent;
            if (parent == null)
                return;

            var rows = new List<ContentPathRow>();

            for (int i = 0; i < parent.Controls.Count; i++)
            {
                if (parent.Controls[i] is ContentPathRow r)
                    rows.Add(r);
            }

            if (rows.Count == 0)
                return;

            rows.Sort((a, b) =>
            {
                var t = a.Top.CompareTo(b.Top);
                if (t != 0) return t;
                return a.Left.CompareTo(b.Left);
            });

            if (rows.Count == 1)
            {
                isFirst = true;
                isLast = true;
                return;
            }

            var idx = rows.IndexOf(this);
            if (idx < 0)
                return;

            isFirst = idx == 0;
            isLast = idx == rows.Count - 1;
        }
        private void ApplyInteractionLock()
        {
            if (enforcingInteractionLock)
                return;

            try
            {
                enforcingInteractionLock = true;

                if (limitInteraction)
                {
                    if (!interactionSnapshotValid)
                    {
                        snapHideEnabled = hideCheckBox.Enabled;
                        interactionSnapshotValid = true;
                    }

                    hideCheckBox.Enabled = false;

                    upBtn.Enabled = false;
                    downBtn.Enabled = false;
                    topBtn.Enabled = false;
                    bottomBtn.Enabled = false;
                }
                else
                {
                    if (interactionSnapshotValid)
                    {
                        hideCheckBox.Enabled = snapHideEnabled;
                        interactionSnapshotValid = false;
                    }

                    ApplyMoveEnabledUnlocked();
                }
            }
            finally
            {
                enforcingInteractionLock = false;
            }
        }

        private void RaiseChanged()
        {
            if (_suppressEvents)
                return;

            Changed?.Invoke(this);
        }

        private void CaptureDefaultLayoutSpecFromThisInstance()
        {
            if (defaultLayoutSpec is not null)
                return;

            var snap = GetLayoutColumnStyleSnapshot();
            if (snap.Length > 0)
                defaultLayoutSpec = snap;
        }

        private sealed class EllipsisButton : Control
        {
            private bool hover;
            private bool down;

            public EllipsisButton()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.StandardClick, true);

                TabStop = false;
                Cursor = Cursors.Hand;
                Size = new Size(24, 24);
                Margin = new Padding(0);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                hover = true;
                Invalidate();
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                hover = false;

                if (!Capture)
                    down = false;

                Invalidate();
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                if (e.Button != MouseButtons.Left)
                    return;

                down = true;
                Capture = true;
                Invalidate();
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                if (e.Button != MouseButtons.Left)
                    return;

                var wasDown = down;

                if (Capture)
                    Capture = false;

                down = false;
                Invalidate();

                if (wasDown && Enabled && ClientRectangle.Contains(e.Location))
                    OnClick(EventArgs.Empty);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var r = ClientRectangle;
                r.Width -= 1;
                r.Height -= 1;

                Color fill;
                Color border;
                Color dots;

                if (!Enabled)
                {
                    fill = Color.FromArgb(246, 247, 249);
                    border = Color.FromArgb(225, 228, 233);
                    dots = Color.FromArgb(155, 165, 175);
                }
                else if (down)
                {
                    fill = Color.FromArgb(230, 236, 244);
                    border = Color.FromArgb(190, 203, 220);
                    dots = Color.FromArgb(40, 50, 60);
                }
                else if (hover)
                {
                    fill = Color.FromArgb(238, 244, 251);
                    border = Color.FromArgb(190, 203, 220);
                    dots = Color.FromArgb(40, 50, 60);
                }
                else
                {
                    fill = Color.White;
                    border = Color.FromArgb(200, 210, 224);
                    dots = Color.FromArgb(40, 50, 60);
                }

                using (var b = new SolidBrush(fill))
                    g.FillRectangle(b, r);

                using (var p = new Pen(border))
                    g.DrawRectangle(p, r);

                var cx = r.Left + r.Width / 2f;
                var cy = r.Top + r.Height / 2f;

                var dotR = 1.6f;
                var gap = 5.0f;

                using var db = new SolidBrush(dots);
                g.FillEllipse(db, cx - gap - dotR, cy - dotR, dotR * 2, dotR * 2);
                g.FillEllipse(db, cx - dotR, cy - dotR, dotR * 2, dotR * 2);
                g.FillEllipse(db, cx + gap - dotR, cy - dotR, dotR * 2, dotR * 2);
            }
        }

        private sealed class LightBorderTextBox : UserControl
        {
            private static readonly Color BorderColor = Color.FromArgb(220, 226, 235);
            private static readonly Color BorderColorFocused = Color.FromArgb(190, 203, 220);

            private readonly TextBox inner;
            private bool focused;

            public LightBorderTextBox()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw, true);

                TabStop = false;
                BackColor = Color.White;
                Margin = new Padding(0);
                Padding = new Padding(6, 0, 6, 0);

                inner = new TextBox
                {
                    BorderStyle = BorderStyle.None,
                    Multiline = false,
                    WordWrap = false,
                    BackColor = Color.White
                };

                inner.TextChanged += (_, __) => OnTextChanged(EventArgs.Empty);
                inner.GotFocus += (_, __) => { focused = true; Invalidate(); };
                inner.LostFocus += (_, __) => { focused = false; Invalidate(); };

                Controls.Add(inner);

                Height = 24;
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public override string Text
            {
                get => inner.Text ?? string.Empty;
                set => inner.Text = value ?? string.Empty;
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public override Font Font
            {
                get => base.Font;
                set
                {
                    base.Font = value;
                    inner.Font = value;
                    LayoutInner();
                }
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                LayoutInner();
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                inner.Focus();
            }

            private void LayoutInner()
            {
                int h = inner.PreferredHeight;
                int y = (Height - h) / 2;
                if (y < 0) y = 0;

                inner.Location = new Point(Padding.Left, y);
                inner.Size = new Size(Math.Max(0, Width - Padding.Left - Padding.Right), h);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.None;

                var r = ClientRectangle;
                r.Width -= 1;
                r.Height -= 1;

                using var b = new SolidBrush(BackColor);
                g.FillRectangle(b, r);

                var bc = focused ? BorderColorFocused : BorderColor;
                using var p = new Pen(bc);
                g.DrawRectangle(p, r);
            }
        }
        private sealed class ArrowButton : Control
        {
            public enum Dir { Up, Down, Top, Bottom }

            private bool hover;
            private bool down;

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Dir Direction { get; set; }

            public ArrowButton(Dir dir)
            {
                Direction = dir;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

                Size = new Size(26, 14);
                Margin = new Padding(0);
                Cursor = Cursors.Hand;
                TabStop = false;
                BackColor = Color.White;
            }

            internal void CancelPress()
            {
                if (!down) return;
                down = false;
                Invalidate();
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hover = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hover = false; down = false; Invalidate(); }
            protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) { down = true; Invalidate(); } }
            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                if (!down) return;

                down = false;
                Invalidate();

                if (Enabled && ClientRectangle.Contains(e.Location))
                    OnClick(EventArgs.Empty);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var r = ClientRectangle;
                r.Width -= 1;
                r.Height -= 1;

                Color fill;
                Color border;

                if (!Enabled)
                {
                    fill = Color.FromArgb(246, 247, 249);
                    border = Color.FromArgb(225, 228, 233);
                }
                else if (down)
                {
                    fill = Color.FromArgb(230, 236, 244);
                    border = Color.FromArgb(190, 203, 220);
                }
                else if (hover)
                {
                    fill = Color.FromArgb(238, 244, 251);
                    border = Color.FromArgb(190, 203, 220);
                }
                else
                {
                    fill = Color.White;
                    border = Color.FromArgb(200, 210, 224);
                }

                using (var b = new SolidBrush(fill))
                    g.FillRectangle(b, r);

                using (var p = new Pen(border))
                    g.DrawRectangle(p, r);

                var fg = Enabled ? Color.FromArgb(40, 50, 60) : Color.FromArgb(155, 165, 175);
                using var tb = new SolidBrush(fg);
                using var stopPen = new Pen(fg, 1.1f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };

                var cx = r.Left + r.Width / 2f;
                var cy = r.Top + r.Height / 2f;

                var triW = 8f;
                var triH = 6f;

                bool up = Direction == Dir.Up || Direction == Dir.Top;
                bool downDir = Direction == Dir.Down || Direction == Dir.Bottom;

                if (Direction == Dir.Top)
                {
                    var y = r.Top + 7f;
                    g.DrawLine(stopPen, r.Left + 5.0f, y, r.Right - 5.0f, y);
                    cy += 1.0f;
                }
                else if (Direction == Dir.Bottom)
                {
                    var y = r.Bottom - 7f;
                    g.DrawLine(stopPen, r.Left + 5.0f, y, r.Right - 5.0f, y);
                    cy -= 1.0f;
                }

                if (up)
                {
                    var pts = new[]
                    {
                        new PointF(cx, cy - triH/2f),
                        new PointF(cx - triW/2f, cy + triH/2f),
                        new PointF(cx + triW/2f, cy + triH/2f)
                    };
                    g.FillPolygon(tb, pts);
                }
                else if (downDir)
                {
                    var pts = new[]
                    {
                        new PointF(cx, cy + triH/2f),
                        new PointF(cx - triW/2f, cy - triH/2f),
                        new PointF(cx + triW/2f, cy - triH/2f)
                    };

                    g.FillPolygon(tb, pts);
                }
            }
        }
        private sealed class BusyOverlay : Control
        {
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string Caption { get; set; } = "Counting...";

            public BusyOverlay()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw, true);

                TabStop = false;
                Cursor = Cursors.WaitCursor;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var g = e.Graphics;

                using (var b = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                    g.FillRectangle(b, ClientRectangle);

                var text = Caption ?? string.Empty;
                if (text.Length == 0)
                    return;

                using var tb = new SolidBrush(Color.FromArgb(235, 255, 255, 255));
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(text, Font ?? SystemFonts.MessageBoxFont, tb, ClientRectangle, sf);
            }

            protected override void OnMouseDown(MouseEventArgs e) { }
            protected override void OnMouseUp(MouseEventArgs e) { }
            protected override void OnMouseMove(MouseEventArgs e) { }
            protected override void OnMouseWheel(MouseEventArgs e) { }
        }

        private static readonly HashSet<string> ExcludedFileNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "thumbs.db"
        };

        private static readonly SemaphoreSlim FileFolderSummaryGate = new(2, 2);

        private BusyOverlay? busyOverlay;
        private Cursor? cursorSnapshot;

        private CancellationTokenSource? countCts;
        private long countReqId;

        private bool busySnapshotValid;
        private bool snapPathEnabled;
        private bool snapBrowseEnabled;

        private void EnsureBusyOverlay()
        {
            if (busyOverlay != null)
                return;

            busyOverlay = new BusyOverlay
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            Controls.Add(busyOverlay);
            busyOverlay.BringToFront();
        }
        private void SetRowBusy(bool on, string caption)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetRowBusy(on, caption)));
                return;
            }

            EnsureBusyOverlay();

            if (on)
            {
                if (!busySnapshotValid)
                {
                    snapPathEnabled = pathTextBox.Enabled;
                    snapBrowseEnabled = browseBtn.Enabled;
                    snapHideEnabledBusy = hideCheckBox.Enabled;
                    busySnapshotValid = true;
                }

                cursorSnapshot ??= Cursor;
                Cursor = Cursors.WaitCursor;

                CancelButtonPresses();

                pathTextBox.Enabled = false;
                browseBtn.Enabled = false;
                hideCheckBox.Enabled = false;

                upBtn.Enabled = false;
                downBtn.Enabled = false;
                topBtn.Enabled = false;
                bottomBtn.Enabled = false;

                busyOverlay!.Caption = caption;
                busyOverlay.Visible = true;
                busyOverlay.BringToFront();
            }
            else
            {
                if (busyOverlay != null)
                    busyOverlay.Visible = false;

                if (cursorSnapshot != null)
                {
                    Cursor = cursorSnapshot;
                    cursorSnapshot = null;
                }

                if (busySnapshotValid)
                {
                    pathTextBox.Enabled = snapPathEnabled;
                    browseBtn.Enabled = snapBrowseEnabled;

                    if (!limitInteraction && !interactionSnapshotValid)
                        hideCheckBox.Enabled = snapHideEnabledBusy;

                    busySnapshotValid = false;
                }

                ApplyInteractionLock();
            }
        }
        private void CancelCountWork()
        {
            try { countCts?.Cancel(); } catch { }
            try { countCts?.Dispose(); } catch { }
            countCts = null;
        }

        private void RequestFileFolderSummaryAsync(string? rootPath)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => RequestFileFolderSummaryAsync(rootPath)));
                return;
            }

            CancelCountWork();
            var reqId = Interlocked.Increment(ref countReqId);
            countCts = new CancellationTokenSource();
            var token = countCts.Token;

            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                filesFoldersLabel.Text = string.Empty;
                SetRowBusy(false, string.Empty);
                return;
            }

            filesFoldersLabel.Text = string.Empty;
            SetRowBusy(true, "Checking Filesystem...");

            _ = RunFileFolderSummaryAsync(rootPath, reqId, token);
        }

        private async Task RunFileFolderSummaryAsync(string rootPath, long reqId, CancellationToken token)
        {
            string result;

            try
            {
                await FileFolderSummaryGate.WaitAsync(token);
                try
                {
                    result = await Task.Run(() => GetFileFolderSummaryCore(rootPath, token), token);
                }
                finally
                {
                    FileFolderSummaryGate.Release();
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                result = string.Empty;
            }

            if (IsDisposed || Disposing)
                return;

            if (reqId != Volatile.Read(ref countReqId))
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed || Disposing)
                        return;

                    if (reqId != Volatile.Read(ref countReqId))
                        return;

                    filesFoldersLabel.Text = result;
                    SetRowBusy(false, string.Empty);
                }));
                return;
            }

            filesFoldersLabel.Text = result;
            SetRowBusy(false, string.Empty);
        }

        private static string GetFileFolderSummaryCore(string rootPath, CancellationToken token)
        {
            int fileCount = 0;
            int folderCount = 0;

            try
            {
                foreach (var _ in Directory.EnumerateDirectories(rootPath))
                {
                    token.ThrowIfCancellationRequested();
                    folderCount++;
                }

                foreach (var file in Directory.EnumerateFiles(rootPath))
                {
                    token.ThrowIfCancellationRequested();

                    var name = System.IO.Path.GetFileName(file);
                    if (ExcludedFileNames.Contains(name))
                        continue;

                    fileCount++;
                }
            }
            catch
            {
                return string.Empty;
            }

            var running = string.Empty;

            if (fileCount > 0)
                running += $"Files: {DatinateHelper.GetReadableNumber(fileCount)}";

            if (folderCount > 0)
            {
                if (!string.IsNullOrWhiteSpace(running))
                    running += Environment.NewLine;

                running += $"Folders: {DatinateHelper.GetReadableNumber(folderCount)}";
            }

            return running;
        }
    }
}
