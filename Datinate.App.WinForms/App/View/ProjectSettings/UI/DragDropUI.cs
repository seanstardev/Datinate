using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Timer = System.Windows.Forms.Timer;

namespace datinate.app
{
    public partial class DragDropUI : UserControl
    {
        private const string DragInstanceFormat = "datinate.app.DragDropUI.InstanceId";
        private static readonly Size DragThreshold = SystemInformation.DragSize;

        private readonly Guid instanceId = Guid.NewGuid();

        private bool sizing;
        private bool pendingUpdate;
        private bool updateQueued;

        private bool dragArmed;
        private Point dragStartScreen;
        private Control? dragRoot;

        private bool enableSelectedPanelReorder = true;

        private readonly HashSet<Control> wiredMouse = new HashSet<Control>();
        private readonly HashSet<Control> wiredDrop = new HashSet<Control>();
        private readonly HashSet<Control> wiredSize = new HashSet<Control>();
        private readonly Dictionary<Control, Control> rootByControl = new Dictionary<Control, Control>();

        private int lastAppliedRowH = -1;

        private bool enableDropPopAnimation = true;

        private void OnAnyDragEnter(object? sender, DragEventArgs e)
        {
            e.Effect = ComputeEffect(sender as Control, e.Data, new Point(e.X, e.Y));
        }

        private void OnAnyDragOver(object? sender, DragEventArgs e)
        {
            e.Effect = ComputeEffect(sender as Control, e.Data, new Point(e.X, e.Y));
        }

        private void OnAnyDragDrop(object? sender, DragEventArgs e)
        {
            var targetPanel = FindOwningPanel(sender as Control);
            if (targetPanel is null)
                return;

            if (!TryGetDragRoot(e.Data, out var root))
                return;

            var sourcePanel = root.Parent as FlowLayoutPanel;
            if (sourcePanel is null)
                return;

            var dropScreenPoint = new Point(e.X, e.Y);

            if (ReferenceEquals(targetPanel, sourcePanel))
            {
                if (!enableSelectedPanelReorder || !ReferenceEquals(targetPanel, selectedPanel))
                    return;

                int oldIdx = selectedPanel.Controls.GetChildIndex(root, throwException: false);

                if (!TryGetInsertIndex(selectedPanel, dropScreenPoint, root, true, out int newIdx, out bool selfDrop))
                    return;

                if (selfDrop || oldIdx == newIdx)
                    return;

                selectedPanel.Controls.SetChildIndex(root, newIdx);

                RequestUpdate();
                QueuePop(root);
                return;
            }

            if (!TryGetInsertIndex(targetPanel, dropScreenPoint, root, false, out int targetIdx, out _))
                return;

            sourcePanel.Controls.Remove(root);
            targetPanel.Controls.Add(root);
            targetPanel.Controls.SetChildIndex(root, targetIdx);

            RequestUpdate();
            QueuePop(root);
        }

        private DragDropEffects ComputeEffect(Control? sender, IDataObject data, Point dropScreenPoint)
        {
            var targetPanel = FindOwningPanel(sender);
            if (targetPanel is null)
                return DragDropEffects.None;

            if (!TryGetDragRoot(data, out var root))
                return DragDropEffects.None;

            var sourcePanel = root.Parent as FlowLayoutPanel;
            if (sourcePanel is null)
                return DragDropEffects.None;

            if (ReferenceEquals(targetPanel, sourcePanel))
            {
                if (!enableSelectedPanelReorder || !ReferenceEquals(targetPanel, selectedPanel))
                    return DragDropEffects.None;

                return TryGetInsertIndex(targetPanel, dropScreenPoint, root, true, out _, out _)
                    ? DragDropEffects.Move
                    : DragDropEffects.None;
            }

            return TryGetInsertIndex(targetPanel, dropScreenPoint, root, false, out _, out _)
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        private bool TryGetInsertIndex(
            FlowLayoutPanel panel,
            Point dropScreenPoint,
            Control draggedRoot,
            bool requireHoveredItem,
            out int insertIndex,
            out bool selfDrop)
        {
            insertIndex = 0;
            selfDrop = false;

            var dropClientPoint = panel.PointToClient(dropScreenPoint);
            var visibleOthers = new List<Control>(Math.Max(0, panel.Controls.Count - 1));
            Control? hovered = null;

            for (int i = 0; i < panel.Controls.Count; i++)
            {
                var c = panel.Controls[i];
                if (!c.Visible)
                    continue;

                if (c.Bounds.Contains(dropClientPoint))
                    hovered = c;

                if (!ReferenceEquals(c, draggedRoot))
                    visibleOthers.Add(c);
            }

            if (hovered is null)
            {
                if (requireHoveredItem)
                    return false;

                insertIndex = visibleOthers.Count;
                int maxIndex = ReferenceEquals(panel, draggedRoot.Parent)
                    ? panel.Controls.Count - 1
                    : panel.Controls.Count;

                if (insertIndex < 0)
                    insertIndex = 0;

                if (insertIndex > maxIndex)
                    insertIndex = maxIndex;

                return true;
            }

            if (ReferenceEquals(hovered, draggedRoot))
            {
                selfDrop = true;
                return false;
            }

            int hoveredIndex = visibleOthers.FindIndex(c => ReferenceEquals(c, hovered));
            if (hoveredIndex < 0)
                return false;

            bool insertBefore = IsBeforeSide(hovered, dropClientPoint);
            insertIndex = insertBefore ? hoveredIndex : hoveredIndex + 1;

            int maxAllowedIndex = ReferenceEquals(panel, draggedRoot.Parent)
                ? panel.Controls.Count - 1
                : panel.Controls.Count;

            if (insertIndex < 0)
                insertIndex = 0;

            if (insertIndex > maxAllowedIndex)
                insertIndex = maxAllowedIndex;

            return true;
        }

        private static bool IsBeforeSide(Control target, Point dropClientPoint)
        {
            var bounds = target.Bounds;

            if (dropClientPoint.Y < bounds.Top)
                return true;

            if (dropClientPoint.Y > bounds.Bottom)
                return false;

            return dropClientPoint.X < (bounds.Left + (bounds.Width / 2));
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(true)]
        public bool EnableDropPopAnimation
        {
            get => enableDropPopAnimation;
            set => enableDropPopAnimation = value;
        }
        private int MinDropAreaHeightPx => ScalePx(120);

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(true)]
        public bool EnableSelectedPanelReorder
        {
            get => enableSelectedPanelReorder;
            set => enableSelectedPanelReorder = value;
        }

        public DragDropUI()
        {
            InitializeComponent();

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, MinDropAreaHeightPx));
            tableLayoutPanel1.ResumeLayout(true);

            Hook(allPanel);
            Hook(selectedPanel);

            SizeChanged += (_, _) => RequestUpdate();
            ParentChanged += (_, _) => RequestUpdate();

            HandleCreated += (_, _) =>
            {
                if (pendingUpdate)
                    pendingUpdate = false;

                RequestUpdate();
            };

            RequestUpdate();
        }
        private void Hook(FlowLayoutPanel p)
        {
            p.WrapContents = true;
            p.AllowDrop = true;

            p.ControlAdded += (_, e) =>
            {
                WireTree(e.Control, e.Control);
                WireDropTree(e.Control);
                WireSizeTree(e.Control);
                RequestUpdate();
            };

            p.ControlRemoved += (_, e) =>
            {
                UnwireTree(e.Control);
                UnwireDropTree(e.Control);
                UnwireSizeTree(e.Control);
                RequestUpdate();
            };

            p.SizeChanged += (_, _) => RequestUpdate();

            p.DragEnter += OnAnyDragEnter;
            p.DragOver += OnAnyDragOver;
            p.DragDrop += OnAnyDragDrop;
            p.DragLeave += OnAnyDragLeave;

            foreach (Control c in p.Controls)
            {
                WireTree(c, c);
                WireDropTree(c);
                WireSizeTree(c);
            }
        }
        public void ClearUI()
        {
            allPanel.Controls.Clear();
            selectedPanel.Controls.Clear();
            RequestUpdate();
        }

        public void AddToAllPanel(Control ctrl)
        {
            allPanel.Controls.Add(ctrl);
            RequestUpdate();
        }

        public IReadOnlyList<Control> GetSelectedItems()
        {
            var list = new List<Control>();
            foreach (var ctrl in selectedPanel.Controls)
            {
                if (ctrl is Control c)
                    list.Add(c);
            }
            return list;
        }

        public void AddToSelectedPanel(Control ctrl)
        {
            selectedPanel.Controls.Add(ctrl);
            RequestUpdate();
        }

        private void RequestUpdate()
        {
            if (IsDisposed)
                return;

            if (!IsHandleCreated)
            {
                pendingUpdate = true;
                return;
            }

            if (updateQueued)
                return;

            updateQueued = true;

            try
            {
                BeginInvoke(new Action(PerformDeferredUpdate));
            }
            catch
            {
                updateQueued = false;
                pendingUpdate = true;
            }
        }
        private void PerformDeferredUpdate()
        {
            updateQueued = false;

            if (IsDisposed || !IsHandleCreated)
                return;

            UpdateContentRowHeight();
        }
        private void UpdateContentRowHeight()
        {
            if (sizing)
                return;

            if (tableLayoutPanel1.RowStyles.Count < 1)
                return;

            if (tableLayoutPanel1.Width <= 1)
                return;

            try
            {
                sizing = true;

                allPanel.PerformLayout();
                selectedPanel.PerformLayout();

                int hAll = MeasureFlowHeight(allPanel);
                int hSel = MeasureFlowHeight(selectedPanel);

                int rowH = Math.Max(hAll, hSel);

                if (rowH < MinDropAreaHeightPx)
                    rowH = MinDropAreaHeightPx;

                if (lastAppliedRowH == rowH)
                    return;

                lastAppliedRowH = rowH;

                var rs = tableLayoutPanel1.RowStyles[0];
                if (rs.SizeType != SizeType.Absolute || Math.Abs(rs.Height - rowH) > 0.5f)
                {
                    tableLayoutPanel1.SuspendLayout();
                    rs.SizeType = SizeType.Absolute;
                    rs.Height = rowH;
                    tableLayoutPanel1.ResumeLayout(true);
                }
            }
            finally
            {
                sizing = false;
            }
        }

        private int MeasureFlowHeight(FlowLayoutPanel p)
        {
            int maxBottom = 0;

            for (int i = 0; i < p.Controls.Count; i++)
            {
                var c = p.Controls[i];
                if (!c.Visible)
                    continue;

                int b = c.Bottom + c.Margin.Bottom;
                if (b > maxBottom)
                    maxBottom = b;
            }

            int h = maxBottom + p.Padding.Bottom;

            if (h < MinDropAreaHeightPx)
                h = MinDropAreaHeightPx;

            h += p.Margin.Vertical;

            if (h < 1)
                h = 1;

            return h;
        }

        private int ScalePx(int px)
            => (int)Math.Round(px * (DeviceDpi / 96f));

        private void WireTree(Control root, Control node)
        {
            rootByControl[node] = root;

            if (!wiredMouse.Contains(node))
            {
                wiredMouse.Add(node);
                node.MouseDown += OnAnyMouseDown;
                node.MouseMove += OnAnyMouseMove;
                node.MouseUp += OnAnyMouseUp;
            }

            foreach (Control child in node.Controls)
                WireTree(root, child);
        }

        private void UnwireTree(Control node)
        {
            if (wiredMouse.Contains(node))
            {
                wiredMouse.Remove(node);
                node.MouseDown -= OnAnyMouseDown;
                node.MouseMove -= OnAnyMouseMove;
                node.MouseUp -= OnAnyMouseUp;
            }

            _ = rootByControl.Remove(node);

            foreach (Control child in node.Controls)
                UnwireTree(child);
        }

        private void WireSizeTree(Control node)
        {
            if (!wiredSize.Contains(node))
            {
                wiredSize.Add(node);
                node.SizeChanged += OnAnyContentSizeChanged;
                node.VisibleChanged += OnAnyContentSizeChanged;
            }

            foreach (Control child in node.Controls)
                WireSizeTree(child);
        }

        private void UnwireSizeTree(Control node)
        {
            if (wiredSize.Contains(node))
            {
                wiredSize.Remove(node);
                node.SizeChanged -= OnAnyContentSizeChanged;
                node.VisibleChanged -= OnAnyContentSizeChanged;
            }

            foreach (Control child in node.Controls)
                UnwireSizeTree(child);
        }

        private void OnAnyContentSizeChanged(object? sender, EventArgs e)
        {
            RequestUpdate();
        }
        private void WireDropTree(Control node)
        {
            if (!wiredDrop.Contains(node))
            {
                wiredDrop.Add(node);
                node.AllowDrop = true;
                node.DragEnter += OnAnyDragEnter;
                node.DragOver += OnAnyDragOver;
                node.DragDrop += OnAnyDragDrop;
                node.DragLeave += OnAnyDragLeave;
            }

            foreach (Control child in node.Controls)
                WireDropTree(child);
        }
        private void UnwireDropTree(Control node)
        {
            if (wiredDrop.Contains(node))
            {
                wiredDrop.Remove(node);
                node.DragEnter -= OnAnyDragEnter;
                node.DragOver -= OnAnyDragOver;
                node.DragDrop -= OnAnyDragDrop;
                node.DragLeave -= OnAnyDragLeave;
            }

            foreach (Control child in node.Controls)
                UnwireDropTree(child);
        }

        private static bool IsInteractive(Control c)
        {
            return c is ButtonBase
                || c is TextBoxBase
                || c is ComboBox
                || c is ListBox
                || c is CheckedListBox
                || c is ListView
                || c is TreeView
                || c is DataGridView
                || c is UpDownBase;
        }

        private void OnAnyMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (sender is not Control c)
                return;

            if (IsInteractive(c))
                return;

            if (!rootByControl.TryGetValue(c, out var root))
                root = c;

            if (!IsDraggableRoot(root))
                return;

            dragArmed = true;
            dragRoot = root;
            dragStartScreen = c.PointToScreen(e.Location);
        }

        private void OnAnyMouseMove(object? sender, MouseEventArgs e)
        {
            if (!dragArmed || dragRoot is null)
                return;

            if ((Control.MouseButtons & MouseButtons.Left) == 0)
            {
                dragArmed = false;
                dragRoot = null;
                return;
            }

            if (sender is not Control c)
                return;

            var now = c.PointToScreen(e.Location);
            if (!HasExceededThreshold(dragStartScreen, now))
                return;

            dragArmed = false;

            var root = dragRoot;
            dragRoot = null;

            StartDrag(root);
        }

        private void OnAnyMouseUp(object? sender, MouseEventArgs e)
        {
            dragArmed = false;
            dragRoot = null;
        }

        private static bool HasExceededThreshold(Point start, Point now)
        {
            return Math.Abs(now.X - start.X) >= DragThreshold.Width
                || Math.Abs(now.Y - start.Y) >= DragThreshold.Height;
        }

        private bool IsDraggableRoot(Control root)
        {
            return ReferenceEquals(root.Parent, allPanel) || ReferenceEquals(root.Parent, selectedPanel);
        }

        private void StartDrag(Control root)
        {
            if (!IsDraggableRoot(root))
                return;

            var data = new DataObject();
            data.SetData(DragInstanceFormat, instanceId.ToString("D"));
            data.SetData(typeof(Control), root);

            DragDropPreviewForm.Initialise(root);

            try
            {
                _ = DoDragDrop(data, DragDropEffects.Move);
            }
            finally
            {
                try { DragDropPreviewForm.Teardown(); } catch { }
                
                RequestUpdate();
                Focus(); // hopefully this fixes scroll jumping in parent containers.
            }
        }


        private void OnAnyDragLeave(object? sender, EventArgs e)
        {
            try { DragDropPreviewForm.UpdateCaption(null, null); } catch { }
        }

        private bool TryGetDragRoot(IDataObject data, out Control root)
        {
            root = null!;

            if (!data.GetDataPresent(DragInstanceFormat))
                return false;

            var idStr = data.GetData(DragInstanceFormat) as string;
            if (idStr is null || !Guid.TryParse(idStr, out var id) || id != instanceId)
                return false;

            if (!data.GetDataPresent(typeof(Control)))
                return false;

            root = data.GetData(typeof(Control)) as Control;
            if (root is null || root.IsDisposed)
                return false;

            if (!IsDraggableRoot(root))
                return false;

            return true;
        }

        private FlowLayoutPanel? FindOwningPanel(Control? c)
        {
            while (c is not null)
            {
                if (ReferenceEquals(c, allPanel))
                    return allPanel;

                if (ReferenceEquals(c, selectedPanel))
                    return selectedPanel;

                c = c.Parent;
            }

            return null;
        }

        private void QueuePop(Control target)
        {
            if (!enableDropPopAnimation)
                return;

            if (IsDisposed || !IsHandleCreated)
                return;

            try
            {
                BeginInvoke(new Action(() => TryPopNow(target)));
            }
            catch
            {
            }
        }
        private void TryPopNow(Control target)
        {
            if (IsDisposed || target.IsDisposed)
                return;

            if (!target.Visible)
                return;

            if (target.Width < 2 || target.Height < 2)
                return;

            if (!target.IsHandleCreated)
                return;

            Bitmap bmp;
            try
            {
                bmp = new Bitmap(target.Width, target.Height, PixelFormat.Format32bppArgb);
                target.DrawToBitmap(bmp, new Rectangle(0, 0, target.Width, target.Height));
            }
            catch
            {
                return;
            }

            Rectangle screenBounds;
            try
            {
                screenBounds = target.RectangleToScreen(new Rectangle(0, 0, target.Width, target.Height));
            }
            catch
            {
                bmp.Dispose();
                return;
            }

            try
            {
                var pop = new PopOverlayForm(bmp, screenBounds);
                pop.Show();
            }
            catch
            {
                bmp.Dispose();
            }
        }

        private sealed class PopOverlayForm : Form
        {
            private const int IntervalMs = 15;
            private const int DurationMs = 170;

            private const int WS_EX_TRANSPARENT = 0x00000020;
            private const int WS_EX_TOOLWINDOW = 0x00000080;
            private const int WS_EX_NOACTIVATE = 0x08000000;

            private readonly Bitmap bmp;
            private readonly Timer tmr;

            private int elapsedMs;
            private float scale = 1f;

            public PopOverlayForm(Bitmap bmp, Rectangle screenBounds)
            {
                this.bmp = bmp;

                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                Bounds = screenBounds;
                TopMost = true;
                BackColor = Color.Magenta;
                TransparencyKey = Color.Magenta;
                DoubleBuffered = true;
                Opacity = 0;

                tmr = new Timer { Interval = IntervalMs };
                tmr.Tick += (_, __) => TickAnim();
            }

            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                elapsedMs = 0;
                scale = 0.92f;
                Opacity = 0.01;
                tmr.Start();
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                try { tmr.Stop(); } catch { }
                try { tmr.Dispose(); } catch { }
                try { bmp.Dispose(); } catch { }
                base.OnFormClosed(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var g = e.Graphics;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                int w = (int)Math.Round(bmp.Width * scale);
                int h = (int)Math.Round(bmp.Height * scale);

                if (w < 1) w = 1;
                if (h < 1) h = 1;

                int x = (ClientSize.Width - w) / 2;
                int y = (ClientSize.Height - h) / 2;

                g.DrawImage(bmp, new Rectangle(x, y, w, h));
            }

            private void TickAnim()
            {
                elapsedMs += IntervalMs;
                float t = elapsedMs / (float)DurationMs;

                if (t >= 1f)
                {
                    Close();
                    return;
                }

                float s;
                if (t < 0.5f)
                    s = Lerp(0.92f, 1.06f, t / 0.5f);
                else
                    s = Lerp(1.06f, 1.00f, (t - 0.5f) / 0.5f);

                scale = s;

                double o;
                if (t < 0.15f) o = t / 0.15f;
                else if (t > 0.85f) o = (1f - t) / 0.15f;
                else o = 1.0;

                Opacity = Math.Max(0.0, Math.Min(0.95, 0.95 * o));

                Invalidate();
            }

            private static float Lerp(float a, float b, float t)
                => a + ((b - a) * t);
        }
    }
}