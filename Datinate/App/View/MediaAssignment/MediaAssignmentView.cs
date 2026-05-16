using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using Timer = System.Windows.Forms.Timer;

namespace datinate.app
{
    public partial class MediaAssignmentView : UserControl, IMediaAssignmentView
    {
        public event Action<IGameFamily, HashSet<string>, string, bool>? MediaMetaUpdateEvt;
        public event Action? LoadMediaInBrowserEvt;

        static Size DescriptorSize = new Size(166, 20);

        private readonly record struct RowStyleState(int Row, SizeType SizeType, float Height);

        private Timer? hoverTimer;
        private bool lastDescriptorGrayscale = false;


        private RowStyleState[]? readOnlyRowCache;
        private Control[]? readOnlyControls;
        private bool lastReadOnlyMode = false;

        private IMediaCollection? mediaCollection = null;
        private bool suppressMetaChangeEvents = false;

        private const string NotesExpandGlyph = "↗";
        private const string NotesCollapseGlyph = "×";

        private const int DescriptorRowIndex = 6;
        private const int NotesRowIndex = 8;

        private Label? notesToggleHitArea;
        private const float DescriptorRowDefaultHeight = 120f;
        private const float NotesRowDefaultHeight = 26f;
        private const float DescriptorRowExpandedHeight = 24f;

        private const float NotesRowExpandedHeight =
            DescriptorRowDefaultHeight + NotesRowDefaultHeight - DescriptorRowExpandedHeight;

        private bool notesAreaExpanded = false;

        public MediaAssignmentView()
        {
            InitializeComponent();

            Facade.RegisterActor(this);

            if (DatinateHelper.IsDesignTime) return;

            CacheReadOnlyLayout();

            ClearView();

            descriptorContainer.CenterVertically = true;
            descriptorContainer.StragglerAlignment = HorizontalAlignment.Left;

            SizeChanged += (_, _) => LayoutScoringOverlay();
            tableLayoutPanel.SizeChanged += (_, _) => LayoutScoringOverlay();
            scoringUI.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.Layout += (_, _) => LayoutScoringOverlay();

            InitialiseDescriptorHoverTracking();

            SetDescriptorGrayscale(true, true);

            InitialiseNotesResizeUi();
        }

        public void SetDescriptorDefinitions(IReadOnlySet<DescriptorDefinitionDTO> descriptorDefinitions)
        {
            Ui(() =>
            {
                SuspendLayout();

                var uis = DescriptorChipUIs;

                foreach (var descriptorUI in uis)
                    descriptorUI.CheckedChanged -= OnDescriptorCheckedChanged;

                descriptorContainer.Controls.Clear();

                foreach (var def in descriptorDefinitions)
                {
                    var chip = new DescriptorChipUI()
                    {
                        Code = def.Code,
                        TagColor = def.Colour,
                        Description = def.Description,
                        Size = DescriptorSize
                    };

                    descriptorContainer.Controls.Add(chip);
                    chip.CheckedChanged += OnDescriptorCheckedChanged;
                }

                ResumeLayout();

                SetDescriptorGrayscale(lastDescriptorGrayscale, true);
                ApplyCheckedDescriptorsFromCurrentMediaCollection();
                SyncScoringDescriptorChips();
            });
        }
        public void SetView(IMediaCollection collection)
        {
            Ui(() =>
            {
                ClearView();

                mediaCollection = collection;
                var descriptorUIs = DescriptorChipUIs;

                suppressMetaChangeEvents = true;

                familyNotesTxt.Text = collection.FamilyNotes;

                foreach (var ui in descriptorUIs)
                    ui.Checked = collection.CheckedDescriptorCodes.Contains(ui.Code);

                SyncScoringDescriptorChips();

                suppressMetaChangeEvents = false;

                UpdateDescriptorGrayscaleFromMouse();
            });
        }
        public void SetScoring(DatGrouperScoring scoring)
        {
            if (lastReadOnlyMode)
                return;

            Ui(() =>
            {
                int total = scoring.MediaDictionary.Count + scoring.ResourceDictionary.Count;

                int hits = 0;

                foreach (var kvp in scoring.ResourceDictionary)
                {
                    if (kvp.Value.HasScore)
                        hits++;
                }

                foreach (var kvp in scoring.MediaDictionary)
                {
                    if (kvp.Value.HasScore)
                        hits++;
                }

                int percent = total <= 0
                    ? 0
                    : (int)Math.Round((hits * 100d) / total, MidpointRounding.AwayFromZero);

                if (percent < 0)
                    percent = 0;
                else if (percent > 100)
                    percent = 100;

                scoringLabel.Text = hits + " / " + total;
                scoringPercentLabel.Text = percent + "%";

                var panelBackColor = scoring.IsScoringExempt
                    ? Color.Orange
                    : SystemColors.Control;

                mediaItemOptionsPanel.BackColor = panelBackColor;
                scoringLeftContainer.BackColor = panelBackColor;

                bool isPerfect = total > 0 && hits == total;

                scoringPercentLabel.BackColor = isPerfect ? Color.LimeGreen : panelBackColor;
                scoringPercentLabel.ForeColor = isPerfect ? Color.White : SystemColors.ControlText;

                scoringLabel.BackColor = panelBackColor;
                scoringLabel.ForeColor = SystemColors.ControlText;

                scoringExemptPanel.Visible = scoring.IsScoringExempt;

                scoringUI.SetUI(scoring);

                scoringUI.Visible = scoringBtn.Checked;

                if (scoringUI.Visible)
                {
                    scoringUI.BringToFront();
                    LayoutScoringOverlay();
                }
            });
        }
        public void SetViewMediaItem(string entryName, RadioSourceDTO radioSource)
        {
            Ui(() =>
            {
                datChipUI.DatKey = radioSource.Id;

                mediaNameTxt.Text =
                    !string.IsNullOrWhiteSpace(radioSource.Source) ? radioSource.Source.Replace("_", ": ") : string.Empty;

                mediaIconUI.ImageKey = radioSource.Source;
                entryNameTxt.Text = entryName;

                var backColour = radioSource.IsRadioResource ? Color.LightBlue : Color.FromArgb(221, 181, 178);

                mediaSelectedContainer.BackColor = backColour;
                cornerTopLeft.BackColor = cornerBottomLeft.BackColor = backColour;
                cornerTopRight.BackColor = cornerBottomRight.BackColor = backColour;

                datChipUI.Visible = true;
            });
        }

        public void SetReadOnlyModeActive(bool readOnlyMode)
        {
            if (readOnlyRowCache == null || readOnlyControls == null)
                CacheReadOnlyLayout();

            if (readOnlyMode == lastReadOnlyMode)
                return;

            lastReadOnlyMode = readOnlyMode;
            Ui(() =>
            {
                SuspendLayout();
                tableLayoutPanel.SuspendLayout();

                try
                {
                    scoringUI.Visible = false;
                    if (readOnlyControls != null)
                    {
                        for (int i = 0; i < readOnlyControls.Length; i++)
                            readOnlyControls[i].Visible = !readOnlyMode;
                    }

                    if (readOnlyRowCache != null)
                    {
                        if (readOnlyMode)
                        {
                            for (int i = 0; i < readOnlyRowCache.Length; i++)
                            {
                                int row = readOnlyRowCache[i].Row;
                                var rs = tableLayoutPanel.RowStyles[row];
                                rs.SizeType = SizeType.Absolute;
                                rs.Height = 0f;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < readOnlyRowCache.Length; i++)
                            {
                                var snap = readOnlyRowCache[i];
                                var rs = tableLayoutPanel.RowStyles[snap.Row];
                                rs.SizeType = snap.SizeType;
                                rs.Height = snap.Height;
                            }
                        }
                    }
                }
                finally
                {
                    tableLayoutPanel.ResumeLayout(true);
                    ResumeLayout(true);
                }
            });
        }

        public void ClearView()
        {

            Ui(() =>
            {
                mediaCollection = null;
                SetNotesAreaExpanded(false);
                scoringPercentLabel.BackColor = BackColor;
                scoringPercentLabel.ForeColor = ForeColor;

                scoringPercentLabel.Text = string.Empty;
                scoringLabel.Text = string.Empty;

                scoringRightContainer.Controls.Clear();
                datChipUI.DatKey = string.Empty;

                mediaNameTxt.Text = string.Empty;

                mediaIconUI.ImageKey = null;
                entryNameTxt.Text = string.Empty;

                mediaItemOptionsPanel.BackColor = SystemColors.Control;


                suppressMetaChangeEvents = true;

                familyNotesTxt.Text = string.Empty;

                var uis = DescriptorChipUIs;
                foreach (var chip in uis)
                {
                    chip.Checked = false;
                }
                suppressMetaChangeEvents = false;

                scoringUI.Visible = false;
                scoringUI.ClearUI();

                datChipUI.Visible = false;
                mediaSelectedContainer.BackColor = Color.Black;
                cornerTopLeft.BackColor = cornerTopRight.BackColor = Color.Black;
                cornerBottomLeft.BackColor = cornerBottomRight.BackColor = Color.Black;

                UpdateDescriptorGrayscaleFromMouse();
            });
        }
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            if (DatinateHelper.IsDesignTime) return;
            LayoutScoringOverlay();
        }

        public void SetMediaCardDragStart()
        {
            if (scoringUI.Visible == false)
            {
                return;
            }
            else
            {
                Ui(() =>
                {
                    scoringUI.Visible = false;
                });
            }
        }

        public void SetMediaCardDragStop()
        {
            if (scoringBtn.Checked && scoringUI.Visible == false)
            {
                Ui(() =>
                {
                    scoringUI.Visible = true;
                });
            }
        }
        private void CacheReadOnlyLayout()
        {
            if (readOnlyRowCache != null && readOnlyControls != null)
                return;

            int startRow = tableLayoutPanel.GetRow(filler2);
            if (startRow < 0)
                startRow = 0;

            List<RowStyleState> rows = [];
            for (int row = startRow; row < tableLayoutPanel.RowCount; row++)
            {
                var rs = tableLayoutPanel.RowStyles[row];
                rows.Add(new RowStyleState(row, rs.SizeType, rs.Height));
            }

            List<Control> controls = [];
            for (int i = 0; i < tableLayoutPanel.Controls.Count; i++)
            {
                var c = tableLayoutPanel.Controls[i];
                int row = tableLayoutPanel.GetRow(c);
                if (row >= startRow)
                    controls.Add(c);
            }

            readOnlyRowCache = [.. rows];
            readOnlyControls = [.. controls];
        }

        private void InitialiseDescriptorHoverTracking()
        {
            if (hoverTimer != null)
                return;

            hoverTimer = new System.Windows.Forms.Timer { Interval = 60 };
            hoverTimer.Tick += (_, _) => UpdateDescriptorGrayscaleFromMouse();
            hoverTimer.Start();

            UpdateDescriptorGrayscaleFromMouse();
        }

        private void UpdateDescriptorGrayscaleFromMouse()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            bool inBottomRegion =
                IsMouseOverControl(mediaItemOptionsPanel) ||
                IsMouseOverControl(descriptorContainer) ||
                IsMouseOverControl(NotesPanel);

            SetDescriptorGrayscale(!inBottomRegion);
        }

        private static bool IsMouseOverControl(Control? c)
        {
            if (c == null || c.IsDisposed || !c.Visible || !c.IsHandleCreated)
                return false;

            var rect = c.RectangleToScreen(c.ClientRectangle);
            return rect.Contains(Control.MousePosition);
        }
        private void SetDescriptorGrayscale(bool grayscale, bool force = false)
        {
            if (!force && lastDescriptorGrayscale == grayscale)
                return;

            lastDescriptorGrayscale = grayscale;

            var uis = DescriptorChipUIs;

            foreach (var chip in uis)
                chip.Grayscale = grayscale;
        }
        private IReadOnlyCollection<DescriptorChipUI> DescriptorChipUIs
        {
            get
            {
                List<DescriptorChipUI> list = [];

                for (int i = 0; i < descriptorContainer.Controls.Count; i++)
                    if (descriptorContainer.Controls[i] is DescriptorChipUI ui)
                        list.Add(ui);

                return list;
            }
        }

        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
                return;
            }

            action();
        }

        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);

            if (DatinateHelper.IsDesignTime) return;

            if (hoverTimer != null)
            {
                try { hoverTimer.Stop(); }
                catch (Exception) { }

                try { hoverTimer.Dispose(); }
                catch (Exception) { }

                hoverTimer = null;
            }
        }

        private void familyNotesTxt_TextChanged(object sender, EventArgs e)
        {
            if (!suppressMetaChangeEvents &&
                !notesAreaExpanded &&
                familyNotesTxt.Focused)
            {
                SetNotesAreaExpanded(true);
            }

            FireMetaUpdate(false);
        }

        private void OnDescriptorCheckedChanged(object? sender, EventArgs e)
        {
            SyncScoringDescriptorChips();

            if (suppressMetaChangeEvents)
                return;

            FireMetaUpdate(true);
        }

        private void FireMetaUpdate(bool descriptorsChanged)
        {

            if (suppressMetaChangeEvents) return;
            if (mediaCollection == null) return;

            var uis = DescriptorChipUIs;
            HashSet<string> checkedDescriptors = new HashSet<string>();

            foreach (var descriptorUI in uis)
            {
                if (descriptorUI.Checked)
                    _ = checkedDescriptors.Add(descriptorUI.Code);
            }

            MediaMetaUpdateEvt?.Invoke(
                mediaCollection.Family,
                checkedDescriptors,
                familyNotesTxt.Text,
                descriptorsChanged);
        }

        private void scoringUI_Click(object sender, EventArgs e)
        {
            scoringBtn.Checked = false;
            ToggleScoringUiVisible();
        }
        private void scoringBtn_Click(object sender, EventArgs e)
            => ToggleScoringUiVisible();

        private void ToggleScoringUiVisible()
        {
            bool show = scoringBtn.Checked;

            scoringUI.Visible = show;

            if (show)
            {
                scoringUI.BringToFront();
                LayoutScoringOverlay();
                return;
            }

            if (scoringBtn.CanFocus)
                scoringBtn.Focus();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (DatinateHelper.IsDesignTime) return;

            LayoutScoringOverlay();
        }
        private void LayoutScoringOverlay()
        {
            if (scoringUI == null || scoringUI.IsDisposed)
                return;

            int floorY = tableLayoutPanel.Top + filler2.Top;

            int preferredHeight = scoringUI.GetPreferredHeightPx();

            int maxHeight = floorY;
            if (maxHeight < 0)
                maxHeight = 0;

            int targetHeight = preferredHeight;
            if (targetHeight > maxHeight)
                targetHeight = maxHeight;
            if (targetHeight < 0)
                targetHeight = 0;

            int x = tableLayoutPanel.Left;
            int width = tableLayoutPanel.ClientSize.Width;
            if (width < 0)
                width = 0;

            int y = floorY - targetHeight;

            scoringUI.SetBounds(x, y, width, targetHeight);

            if (scoringUI.Visible)
                scoringUI.BringToFront();

            scoringUI.RefreshViewportLayout();
        }

        private void loadMediaInBrowserBtn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoadMediaInBrowserEvt?.Invoke();
        }

        private void REAL_browserBtn_Click(object sender, EventArgs e)
        {
            LoadMediaInBrowserEvt?.Invoke();
        }

        private void browserBtn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ToggleNotesAreaFromButton();
        }
        private void ToggleNotesAreaFromButton()
        {
            SetNotesAreaExpanded(!notesAreaExpanded, !notesAreaExpanded);
        }

        private void NotesToggleHitArea_Click(object? sender, EventArgs e)
        {
            ToggleNotesAreaFromButton();
        }

        private void FamilyNotesTxt_MouseDown(object? sender, MouseEventArgs e)
        {
            if (notesAreaExpanded)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            SetNotesAreaExpanded(true, true);
        }

        private void FamilyNotesTxt_Enter(object? sender, EventArgs e)
        {
            if (notesAreaExpanded)
                return;

            if (Control.MouseButtons != MouseButtons.Left)
                return;

            SetNotesAreaExpanded(true, true);
        }
        private void InitialiseNotesResizeUi()
        {
            browserBtn.ClickThrough = true;
            browserBtn.AutoSize = false;
            browserBtn.Location = new Point(8, -5);
            browserBtn.Size = new Size(30, 26);
            browserBtn.TextAlign = ContentAlignment.MiddleCenter;
            browserBtn.TabStop = false;

            if (notesToggleHitArea == null)
            {
                notesToggleHitArea = new Label();
                notesToggleHitArea.Name = "notesToggleHitArea";
                notesToggleHitArea.BackColor = Color.Transparent;
                notesToggleHitArea.Cursor = Cursors.Hand;
                notesToggleHitArea.Location = new Point(4, 0);
                notesToggleHitArea.Size = new Size(38, 26);
                notesToggleHitArea.TabStop = false;
                notesToggleHitArea.Click += NotesToggleHitArea_Click;

                NotesPanel.Controls.Add(notesToggleHitArea);
                notesToggleHitArea.SendToBack();
                browserBtn.BringToFront();
            }

            NotesPanel.MouseDown += NotesPanelMouseDown;
            familyNotesTxt.MouseDown += FamilyNotesTxt_MouseDown;
            familyNotesTxt.Enter += FamilyNotesTxt_Enter;

            UpdateNotesAreaUi();
        }
        private void NotesPanelMouseDown(object? sender, MouseEventArgs e)
        {
            if (notesAreaExpanded)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            SetNotesAreaExpanded(true, true);
        }

        private void SetNotesAreaExpanded(bool expand, bool focusNotes = false)
        {
            if (DatinateHelper.IsDesignTime)
                return;

            notesAreaExpanded = expand;

            float descriptorHeight = expand ? DescriptorRowExpandedHeight : DescriptorRowDefaultHeight;
            float notesHeight = expand ? NotesRowExpandedHeight : NotesRowDefaultHeight;

            SuspendLayout();
            tableLayoutPanel.SuspendLayout();

            try
            {
                tableLayoutPanel.RowStyles[DescriptorRowIndex].SizeType = SizeType.Absolute;
                tableLayoutPanel.RowStyles[DescriptorRowIndex].Height = descriptorHeight;

                tableLayoutPanel.RowStyles[NotesRowIndex].SizeType = SizeType.Absolute;
                tableLayoutPanel.RowStyles[NotesRowIndex].Height = notesHeight;

                descriptorContainer.MinimumSize = new Size(0, (int)Math.Round(descriptorHeight));
                descriptorContainer.MaximumSize = new Size(0, (int)Math.Round(descriptorHeight));

                NotesPanel.MinimumSize = new Size(0, (int)Math.Round(notesHeight));
                NotesPanel.MaximumSize = new Size(0, (int)Math.Round(notesHeight));
            }
            finally
            {
                tableLayoutPanel.ResumeLayout(true);
                ResumeLayout(true);
            }

            tableLayoutPanel.PerformLayout();
            PerformLayout();

            UpdateNotesAreaUi();
            SyncNotesTextBoxLayout();

            Invalidate(true);

            if (focusNotes && familyNotesTxt.CanFocus)
            {
                familyNotesTxt.Focus();
                familyNotesTxt.SelectionStart = familyNotesTxt.TextLength;
                familyNotesTxt.SelectionLength = 0;
            }
        }
        private void UpdateNotesAreaUi()
        {
            browserBtn.Text = notesAreaExpanded ? NotesCollapseGlyph : NotesExpandGlyph;
            browserBtn.Cursor = Cursors.Hand;

            descriptorContainer.Cursor = Cursors.Default;
            NotesPanel.Cursor = notesAreaExpanded ? Cursors.Default : Cursors.IBeam;

            if (notesToggleHitArea != null)
                notesToggleHitArea.Cursor = Cursors.Hand;

            SyncNotesTextBoxLayout();
        }
        private void SyncNotesTextBoxLayout()
        {
            if (familyNotesTxt == null || familyNotesTxt.IsDisposed)
                return;

            int left = 43;
            int width = NotesPanel.ClientSize.Width - left - 1;
            if (width < 0)
                width = 0;

            familyNotesTxt.Multiline = true;
            familyNotesTxt.ScrollBars = ScrollBars.Vertical;

            if (notesAreaExpanded)
            {
                int height = NotesPanel.ClientSize.Height - 2;
                if (height < 24)
                    height = 24;

                familyNotesTxt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                familyNotesTxt.SetBounds(left, 0, width, height);
            }
            else
            {
                familyNotesTxt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                familyNotesTxt.SetBounds(left, 1, width, 24);
            }
        }
        private void ApplyCheckedDescriptorsFromCurrentMediaCollection()
        {
            if (mediaCollection == null)
                return;

            foreach (var ui in DescriptorChipUIs)
                ui.Checked = mediaCollection.CheckedDescriptorCodes.Contains(ui.Code);
        }
        private void SyncScoringDescriptorChips()
        {
            scoringRightContainer.SuspendLayout();

            try
            {
                scoringRightContainer.Controls.Clear();

                foreach (var chip in DescriptorChipUIs)
                {
                    if (!chip.Checked)
                        continue;

                    var bmp = DescriptorChipUtil.GetDescriptorBitmap(chip.Code);
                    if (bmp == null)
                        continue;

                    scoringRightContainer.Controls.Add(new PictureBox
                    {
                        Image = bmp,
                        Size = bmp.Size,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        TabStop = false,
                        Tag = chip.Code
                    });
                }
            }
            finally
            {
                scoringRightContainer.ResumeLayout(true);
            }
        }
    }
}
