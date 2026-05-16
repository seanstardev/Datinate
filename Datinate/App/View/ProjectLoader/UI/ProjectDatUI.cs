using com.RADIO.Datinate.RMVC.Shared;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class ProjectDatUI : UserControl
    {
        private static readonly ToolTip CommentToolTip = new ToolTip();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBeingRemoved { get; private set; } = false;

        private string? comment;

        public enum ORDER_CHANGE_ENUM
        {
            NONE,
            MOVE_LEFT,
            MOVE_RIGHT,
            MOVE_TO_FIRST,
            MOVE_TO_LAST
        }

        private const string DEFAULT_NONE = "[None]";

        public event Action<ProjectDatUI, Control>? RemovedEvt;
        public event Action<ProjectDatUI, Control>? LoadExpressionsEvt;
        public event Action<ProjectDatUI, Control>? OrderChangeEvt;
        public event Action<ProjectDatUI, Control>? EditExpressionsEvt;
        public event Action<ProjectDatUI, Control>? CommentEditEvt;
        public event Action<ProjectDatUI, Control>? CommentPreviewStartEvt;
        public event Action<ProjectDatUI, Control>? CommentPreviewEndEvt;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ORDER_CHANGE_ENUM LastOrderChangeAction { get; private set; } = ORDER_CHANGE_ENUM.NONE;

        private string datFullpath = string.Empty;
        private string expressionsFullpath = string.Empty;
        private DAT_GROUP_ENUM datEnum = DAT_GROUP_ENUM.NOT_SET;

        private DatGrouperProjectEntry? projectEntry;

        private enum ActionBtnKind { Neutral, Accent, Danger }

        private sealed class BtnState
        {
            public ActionBtnKind Kind;
            public bool Hover;
            public bool Down;
        }

        private readonly Dictionary<Button, BtnState> btnStates = new Dictionary<Button, BtnState>(16);

        private static bool IsDesignTime() => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public ProjectDatUI()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();

            // Keep designer stable: don't pull app resources while hosted by VS designer.
            if (!IsDesignTime())
                mediaIconUI.Image = Datinate.Properties.Resources.media_icons_ROM;

            commentBtn.MouseEnter += OnCommentBtnHover;
            commentBtn.MouseLeave += OnCommentBtnLeave;

            ApplyStyling();
        }

        public void SetUI(DatGrouperProjectEntry projectEntry)
        {
            editExpressionsBtn.Enabled = false;

            subsetEntryLabel.Text = subsetPathLabel.Text = string.Empty;
            this.projectEntry = projectEntry;

            if (projectEntry.CollectionSetEnum != COLLECTION_SET_ENUM.Software)
                filtersStripPanel.Visible = false;

            SetComment(projectEntry.Comment ?? string.Empty);

            datFullpath = projectEntry.DatFullpath;

            string collectionTxt = projectEntry.CollectionSetEnum switch
            {
                COLLECTION_SET_ENUM.Media => "Media",
                COLLECTION_SET_ENUM.Resource => "Resource",
                COLLECTION_SET_ENUM.Software => "Software",
                COLLECTION_SET_ENUM.Support => "Support",
                _ => "Software"
            };

            var desc = projectEntry.InternalDescriptor ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(desc))
                collectionTxt += ": " + desc;

            // TODO: anything should work here, but that is not satisfactory
            if (projectEntry.CollectionSetEnum == COLLECTION_SET_ENUM.Resource)
                mediaIconUI.ImageKey = "Resource_" + Guid.NewGuid().ToString();

            else if (projectEntry.CollectionSetEnum == COLLECTION_SET_ENUM.Media)
            {
                mediaIconUI.ImageKey = desc;
            }

            collectionTypeLabel.Text = collectionTxt;

            DatSubsetFilter? subset = projectEntry.DatSubsetFilter;

            if (subset != null && !(string.IsNullOrWhiteSpace(subset.Entry)))
            {
                subsetEntryLabel.Text = subset.Entry;

                if (!string.IsNullOrWhiteSpace(subset.Path))
                    subsetPathLabel.Text = subset.Path;
                else
                    subsetPathLabel.Text = DEFAULT_NONE;
            }
            else
            {
                subsetEntryLabel.Visible = subsetPathLabel.Visible = subsetArrow.Visible = false;
            }

            datEnum = projectEntry.DatGroupEnum;

            datNameLabel.Text = projectEntry.NameWithoutExt;
            datChipUI.DatKey = projectEntry.DatGroupEnum.ToString();
            friendlyNameTextBox.Text = projectEntry.FriendlyName;

            expressionsFullpath = projectEntry.ExpressionsXmlFullpath;

            if (string.IsNullOrWhiteSpace(expressionsFullpath))
            {
                clearExpressionsBtn2.Enabled = false;
                editExpressionsBtn.Enabled = false;
            }
            else
            {
                expressionsNameTextBox.Text = Path.GetFileNameWithoutExtension(expressionsFullpath);

                clearExpressionsBtn2.Enabled = true;
                editExpressionsBtn.Enabled = true;
            }
        }

        public DatGrouperProjectEntry? GetData()
        {
            if (projectEntry == null) return null;

            string pointer;
            string? internalDescriptor;

            // Software:
            if (projectEntry.CollectionSetEnum == COLLECTION_SET_ENUM.Software)
            {
                pointer = DatinateHelper.BuildPointerId(
                    COLLECTION_SET_ENUM.Software,
                    datEnum,
                    null,
                    projectEntry.DatSubsetFilter,
                    friendlyNameTextBox.Text.Trim());

                internalDescriptor = null;
            }

            // Resource:
            else if (projectEntry.CollectionSetEnum == COLLECTION_SET_ENUM.Resource)
            {
                var typeEnum = projectEntry.InternalDescriptor?.ToString() ?? string.Empty;

                pointer = DatinateHelper.BuildPointerId(
                    COLLECTION_SET_ENUM.Resource,
                    datEnum,
                    typeEnum,
                    projectEntry.DatSubsetFilter,
                    friendlyNameTextBox.Text.Trim());

                internalDescriptor = projectEntry.InternalDescriptor;
            }

            // Media
            else
            {
                string typeStr = projectEntry.InternalDescriptor == null
                    ? MEDIA_TYPE_ENUM.NOT_SET.ToString() :
                    projectEntry.InternalDescriptor.ToString();

                var typeEnum = DatinateHelper.GetEnumFromString<MEDIA_TYPE_ENUM>(typeStr);

                pointer = DatinateHelper.BuildPointerId(
                    COLLECTION_SET_ENUM.Media,
                    datEnum,
                    typeEnum.ToString(),
                    projectEntry.DatSubsetFilter,
                    friendlyNameTextBox.Text);

                internalDescriptor = projectEntry.InternalDescriptor;
            }

            string expressionsPathCleaned =
                expressionsFullpath == DEFAULT_NONE || string.IsNullOrWhiteSpace(expressionsFullpath)
                    ? string.Empty
                    : expressionsFullpath;

            return new DatGrouperProjectEntry(
                projectEntry.CollectionSetEnum,
                datFullpath,
                pointer,
                expressionsPathCleaned,
                datEnum,
                friendlyNameTextBox.Text,
                projectEntry!.DatSubsetFilter,
                internalDescriptor,
                GetComment(),
                projectEntry.ContentPath,
                projectEntry.HideInUi
            );
        }

        private void ApplyStyling()
        {
            RegisterActionButton(removeBtn, ActionBtnKind.Danger);
            RegisterActionButton(clearExpressionsBtn2, ActionBtnKind.Danger);
            RegisterActionButton(firstBtn, ActionBtnKind.Neutral);
            RegisterActionButton(lastBtn, ActionBtnKind.Neutral);
            RegisterActionButton(rightBtn, ActionBtnKind.Neutral);
            RegisterActionButton(leftBtn, ActionBtnKind.Neutral);
            RegisterActionButton(loadExpressionsBtn, ActionBtnKind.Neutral);
            RegisterActionButton(commentBtn, ActionBtnKind.Neutral);
            RegisterActionButton(editExpressionsBtn, ActionBtnKind.Neutral);
        }

        private void RegisterActionButton(Button btn, ActionBtnKind kind)
        {
            btnStates[btn] = new BtnState { Kind = kind };

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseDownBackColor = Color.Empty;
            btn.FlatAppearance.MouseOverBackColor = Color.Empty;
            btn.UseVisualStyleBackColor = false;
            btn.TabStop = false;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.Padding = new Padding(0);
            btn.Margin = new Padding(0);
            btn.Font = new Font("Segoe UI Semibold", 8.0f, FontStyle.Bold);

            btn.MouseEnter += ActionBtn_MouseEnter;
            btn.MouseLeave += ActionBtn_MouseLeave;
            btn.MouseDown += ActionBtn_MouseDown;
            btn.MouseUp += ActionBtn_MouseUp;
            btn.EnabledChanged += ActionBtn_EnabledChanged;

            UpdateActionButtonVisual(btn);
        }

        private void ActionBtn_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (!btnStates.TryGetValue(btn, out var st)) return;
            st.Hover = true;
            UpdateActionButtonVisual(btn);
        }

        private void ActionBtn_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (!btnStates.TryGetValue(btn, out var st)) return;
            st.Hover = false;
            st.Down = false;
            UpdateActionButtonVisual(btn);
        }

        private void ActionBtn_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (sender is not Button btn) return;
            if (!btnStates.TryGetValue(btn, out var st)) return;
            st.Down = true;
            UpdateActionButtonVisual(btn);
        }

        private void ActionBtn_MouseUp(object? sender, MouseEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!btnStates.TryGetValue(btn, out var st)) return;
            st.Down = false;
            UpdateActionButtonVisual(btn);
        }

        private void ActionBtn_EnabledChanged(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            UpdateActionButtonVisual(btn);
        }

        private void UpdateActionButtonVisual(Button btn)
        {
            if (!btnStates.TryGetValue(btn, out var st)) return;

            Color border;
            Color back;
            Color fore;

            if (!btn.Enabled)
            {
                back = Color.FromArgb(246, 248, 251);
                border = Color.FromArgb(225, 231, 238);
                fore = Color.FromArgb(150, 160, 172);
            }
            else
            {
                switch (st.Kind)
                {
                    case ActionBtnKind.Danger:
                        back = st.Down ? Color.FromArgb(255, 218, 218) : st.Hover ? Color.FromArgb(255, 232, 232) : Color.FromArgb(252, 247, 247);
                        border = st.Hover || st.Down ? Color.FromArgb(230, 160, 160) : Color.FromArgb(232, 205, 205);
                        fore = Color.FromArgb(150, 52, 52);
                        break;

                    case ActionBtnKind.Accent:
                        back = st.Down ? Color.FromArgb(214, 232, 250) : st.Hover ? Color.FromArgb(232, 244, 255) : Color.FromArgb(246, 250, 255);
                        border = st.Hover || st.Down ? Color.FromArgb(160, 195, 232) : Color.FromArgb(210, 226, 242);
                        fore = Color.FromArgb(40, 90, 150);
                        break;

                    default:
                        back = st.Down ? Color.FromArgb(226, 235, 246) : st.Hover ? Color.FromArgb(238, 245, 252) : Color.FromArgb(248, 250, 253);
                        border = st.Hover || st.Down ? Color.FromArgb(180, 205, 232) : Color.FromArgb(220, 228, 238);
                        fore = Color.FromArgb(55, 63, 74);
                        break;
                }
            }

            btn.BackColor = back;
            btn.ForeColor = fore;
            btn.FlatAppearance.BorderColor = border;
        }

        public void UpdateByIndex(int index, bool isLastUI)
        {
            firstBtn.Visible = leftBtn.Visible = index != 0;

            lastBtn.Visible = rightBtn.Visible = !isLastUI;

            if (index == 0 && projectEntry?.CollectionSetEnum == COLLECTION_SET_ENUM.Software)
                BackColor = UIHelper.POP_COLOUR;
            else
                BackColor = Color.White;

            datNameLabel.BackColor = BackColor;
            indexLabel.Text = "#" + (index + 1);
        }

        public void SetExpressionsFullpath(string expressionsXmlFullpath)
        {
            expressionsFullpath = expressionsXmlFullpath;
            expressionsNameTextBox.Text = Path.GetFileNameWithoutExtension(expressionsXmlFullpath);
            clearExpressionsBtn2.Enabled = true;
            editExpressionsBtn.Enabled = true;
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            if (Parent == null) return;
            InvokRemovedEvt();
            RemovedEvt?.Invoke(this, Parent);
        }

        private void expressionsBtn_Click(object sender, EventArgs e)
        {
            if (Parent == null) return;
            LoadExpressionsEvt?.Invoke(this, Parent);
        }

        private void clearExpressionsBtn_Click(object sender, EventArgs e)
        {
            editExpressionsBtn.Enabled = false;
            clearExpressionsBtn2.Enabled = false;
            expressionsFullpath = string.Empty;
            expressionsNameTextBox.Text = string.Empty;
        }
        
        // NOTE: Start Move Events..
        //
        //

        private void setAsParentBtn_Click(object sender, EventArgs e)
        {
            if (Parent == null) return;
            UnrenderUiElementsForTransition();
            LastOrderChangeAction = ORDER_CHANGE_ENUM.MOVE_TO_FIRST;
            OrderChangeEvt?.Invoke(this, Parent);
        }

        private void setAsLastBtn_Click(object sender, EventArgs e)
        {
            if (Parent == null) return;
            UnrenderUiElementsForTransition();
            LastOrderChangeAction = ORDER_CHANGE_ENUM.MOVE_TO_LAST;
            OrderChangeEvt?.Invoke(this, Parent);

        }

        private void leftBtn_Click(object sender, EventArgs e)
        {
            if (Parent == null) return;
            UnrenderUiElementsForTransition();
            LastOrderChangeAction = ORDER_CHANGE_ENUM.MOVE_LEFT;
            OrderChangeEvt?.Invoke(this, Parent);
        }

        private void rightBtn_Click(object sender, EventArgs e)
        {
            if (Parent == null) return;
            UnrenderUiElementsForTransition();
            LastOrderChangeAction = ORDER_CHANGE_ENUM.MOVE_RIGHT;
            OrderChangeEvt?.Invoke(this, Parent);
        }

        //
        //
        // NOTE: End Move Events.

        private void UnrenderUiElementsForTransition()
        {
            leftBtn.Visible = rightBtn.Visible = lastBtn.Visible = firstBtn.Visible = false;
            BackColor = datNameLabel.BackColor = Color.White;

            datNameLabel.BackColor = BackColor;
            indexLabel.Text = string.Empty;
        }

        private void loadExpressionsBtn_Click(object sender, EventArgs e)
        {
            if (Parent == null) return;
            EditExpressionsEvt?.Invoke(this, Parent);
        }

        public string GetComment()
        {
            return string.IsNullOrWhiteSpace(comment) ? string.Empty : comment!;
        }

        public string GetCommentName()
        {
            var str = collectionTypeLabel.Text;
            if (!string.IsNullOrWhiteSpace(projectEntry?.InternalDescriptor))
                str += ": " + projectEntry.InternalDescriptor;

            if (!string.IsNullOrWhiteSpace(friendlyNameTextBox.Text))
                str += ": " + friendlyNameTextBox.Text.Trim();

            return str;
        }
        public void SetComment(string comment)
        {
            this.comment = comment;

            string tip = "{ This Reference does not have a Comment attached. Click here to create one. }";
            if (!string.IsNullOrWhiteSpace(this.comment))
            {
                tip = "Comment: " + Environment.NewLine + Environment.NewLine;
                tip += "\"" + this.comment + "\"";
            }

            CommentToolTip.SetToolTip(commentBtn, tip);
        }

        private void OnCommentBtnHover(object? sender, EventArgs e)
        {
            if (Parent == null) return;
            CommentPreviewStartEvt?.Invoke(this, Parent);
        }

        private void OnCommentBtnLeave(object? sender, EventArgs e)
        {
            if (Parent == null) return;
            CommentPreviewEndEvt?.Invoke(this, Parent);
        }

        private void OnCommentBtnClicked(object? sender, EventArgs e)
        {
            if (Parent == null) return;
            CommentEditEvt?.Invoke(this, Parent);
        }

        protected void InvokRemovedEvt()
        {
            IsBeingRemoved = true;

            commentBtn.MouseLeave -= OnCommentBtnLeave;
            commentBtn.MouseEnter -= OnCommentBtnHover;
            commentBtn.Click -= OnCommentBtnClicked;
        }
        // -------------------- Drag/drop visuals (FlowLayout reorder) --------------------

        private bool dragPlaceholderActive;
        private List<(Control Ctrl, bool WasVisible)>? dragSavedChildVis;
        private bool dragHoverActive;

        private double dropPulseT;
        private System.Windows.Forms.Timer? dropPulseTimer;

        // NOTE: Size-lock state for placeholder mode (prevents FlowLayoutPanel reflow during drag)
        private bool dragPlaceholderOldAutoSize;
        private AutoSizeMode dragPlaceholderOldAutoSizeMode;
        private Size dragPlaceholderOldMin;
        private Size dragPlaceholderOldMax;
        private Size dragPlaceholderFixedSize;

        public void BeginDropPulse(int durationMs = 260)
        {
            if (IsDesignTime())
                return;

            dropPulseTimer?.Stop();
            dropPulseTimer?.Dispose();
            dropPulseTimer = null;

            dropPulseT = 1.0;

            var sw = Stopwatch.StartNew();
            dropPulseTimer = new System.Windows.Forms.Timer { Interval = 15 };
            dropPulseTimer.Tick += (_, __) =>
            {
                var t = sw.Elapsed.TotalMilliseconds / Math.Max(1, durationMs);
                if (t >= 1.0)
                {
                    dropPulseT = 0.0;
                    dropPulseTimer?.Stop();
                    dropPulseTimer?.Dispose();
                    dropPulseTimer = null;
                    Invalidate();
                    return;
                }

                // Ease-out.
                var x = 1.0 - t;
                dropPulseT = x * x * x;

                Invalidate();
            };
            dropPulseTimer.Start();
        }
        public void EndDragPlaceholderMode()
        {
            if (IsDesignTime())
                return;

            if (!dragPlaceholderActive)
                return;

            dragPlaceholderActive = false;

            // Restore child visibility exactly as it was
            SuspendLayout();
            try
            {
                if (dragSavedChildVis != null)
                {
                    for (int i = 0; i < dragSavedChildVis.Count; i++)
                    {
                        var (ctrl, wasVisible) = dragSavedChildVis[i];
                        if (!ctrl.IsDisposed)
                            ctrl.Visible = wasVisible;
                    }
                }
            }
            finally
            {
                ResumeLayout(false);
            }

            dragSavedChildVis = null;

            // Restore sizing behaviour.
            MinimumSize = dragPlaceholderOldMin;
            MaximumSize = dragPlaceholderOldMax;
            AutoSizeMode = dragPlaceholderOldAutoSizeMode;
            AutoSize = dragPlaceholderOldAutoSize;

            Invalidate();
        }

        public void SetDragHover(bool hover)
        {
            if (dragHoverActive == hover)
                return;

            dragHoverActive = hover;
            Invalidate();
        }

        private bool dragPlaceholderDrawOutline = true;

        public void BeginDragPlaceholderMode(bool showOutline = true)
        {
            if (IsDesignTime())
                return;

            if (dragPlaceholderActive)
            {
                // allow callers to change whether outline is shown while already active
                dragPlaceholderDrawOutline = showOutline;
                Invalidate();
                return;
            }

            dragPlaceholderActive = true;
            dragPlaceholderDrawOutline = showOutline;

            // Lock size so hiding children does NOT cause FlowLayoutPanel to re-flow early.
            dragPlaceholderFixedSize = Size;
            dragPlaceholderOldAutoSize = AutoSize;
            dragPlaceholderOldAutoSizeMode = AutoSizeMode;
            dragPlaceholderOldMin = MinimumSize;
            dragPlaceholderOldMax = MaximumSize;

            AutoSize = false;
            AutoSizeMode = AutoSizeMode.GrowOnly;
            MinimumSize = dragPlaceholderFixedSize;
            MaximumSize = dragPlaceholderFixedSize;
            Size = dragPlaceholderFixedSize;

            // Save and hide child visibility (don’t destroy the original state)
            dragSavedChildVis = new List<(Control Ctrl, bool WasVisible)>(Controls.Count);

            SuspendLayout();
            try
            {
                for (int i = 0; i < Controls.Count; i++)
                {
                    var c = Controls[i];
                    dragSavedChildVis.Add((c, c.Visible));
                    c.Visible = false;
                }
            }
            finally
            {
                ResumeLayout(false);
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Borders must render over children: keep them simple and cheap.
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;
            if (rect.Width <= 2 || rect.Height <= 2)
                return;

            // Placeholder when dragging OR during swap. For swap we want it BLANK.
            if (dragPlaceholderActive)
            {
                if (dragPlaceholderDrawOutline)
                {
                    rect.Inflate(-2, -2);
                    using var pen = new Pen(Color.FromArgb(160, 120, 120, 120), 2f)
                    {
                        DashStyle = DashStyle.Dot,
                        DashCap = DashCap.Round,
                        LineJoin = LineJoin.Round
                    };
                    g.DrawRectangle(pen, rect);
                }

                return;
            }
            if (dragHoverActive)
            {
                var r = ClientRectangle;
                r.Inflate(-2, -2);

                // subtle fill
                using (var br = new SolidBrush(Color.FromArgb(18, 70, 130, 220)))
                    g.FillRectangle(br, r);

                // crisp 1px border
                using (var pen = new Pen(Color.FromArgb(120, 70, 130, 220), 1f))
                    g.DrawRectangle(pen, r);

                // don’t return; let drop pulse still draw if active
            }


            // Drop pulse border.
            if (dropPulseT > 0.0)
            {
                var alpha = (int)Math.Round(255.0 * Math.Min(1.0, Math.Max(0.0, dropPulseT)));
                using var pen = new Pen(Color.FromArgb(alpha, 20, 100, 220), 3f)
                {
                    LineJoin = LineJoin.Round
                };

                var pulseRect = new Rectangle(2, 2, Width - 5, Height - 5);
                if (pulseRect.Width > 0 && pulseRect.Height > 0)
                    g.DrawRectangle(pen, pulseRect);
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool IsDragPlaceholderActive => dragPlaceholderActive;

        internal void RevealPlaceholderChildrenForSnapshot()
        {
            if (!dragPlaceholderActive)
                return;

            SuspendLayout();
            for (int i = 0; i < Controls.Count; i++)
                Controls[i].Visible = true;
            ResumeLayout(false);
        }

        internal void RehidePlaceholderChildrenAfterSnapshot()
        {
            if (!dragPlaceholderActive)
                return;

            SuspendLayout();
            for (int i = 0; i < Controls.Count; i++)
                Controls[i].Visible = false;
            ResumeLayout(false);
        }
    }
}
