using Datinate.Properties;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.InteropServices;

namespace datinate.app
{
    public class WindowSizePresetBar : Control
    {
        private const int MaximisePreviewTopCorrectionPx = 7;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static bool DebugRender { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static bool EnableOverlays { get; set; } = false;

        private static readonly object DebugDisableSync = new();
        private static bool debugDisable;
        private static long debugDisableEpoch;
        private static long overlayRenderPrimitiveCounter;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static bool DebugDisable
        {
            get
            {
                lock (DebugDisableSync)
                    return debugDisable;
            }
            set
            {
                lock (DebugDisableSync)
                {
                    if (debugDisable == value)
                        return;

                    debugDisable = value;

                    if (value)
                        debugDisableEpoch++;
                }
            }
        }

        private readonly bool debugDisableLatchedAtCreation;
        private readonly long debugDisableEpochAtCreation;

        private bool debugDisableVisualStateApplied;
        private bool debugDisableRestoreVisible = true;
        private bool debugDisableRestoreEnabled = true;

        private ResizeOverlayForm? overlayForm;
        private Rectangle? overlayPreviewBounds;
        private bool transitionOverlayActive;
        private bool pendingMaximiseOverlay;
        private bool overlayPaintPumpActive;
        private long overlayRenderPrimitiveValue;

        private const int CornerRadiusPx = 10;
        private const int SegmentGapPx = 4;
        private const int HorizontalPaddingPx = 10;
        private const int FontSizePx = 9;
        private const int OverlayHideDelayMs = 180;
        private const int MaxSelectedValue = 100;
        private const int MinSizeValue = 50;

        private const int WM_SYSCOMMAND = 0x0112;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_NCLBUTTONDBLCLK = 0x00A3;

        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_RESTORE = 0xF120;

        private const int HTCAPTION = 2;
        private const int HTMAXBUTTON = 9;

        // Heuristic tuning for preserving a visible top toolbar/header band.
        private const int TopReservedMinWidthPercent = 55;
        private const int TopReservedLooseTopThresholdPx = 18;
        private const int TopReservedExtraPadPx = 2;

        
        private readonly int[] values = [MinSizeValue, 75, MaxSelectedValue];

        private int hoveredIndex = -1;
        private int pressedIndex = -1;
        private int selectedPercent = MinSizeValue;
        private int lastNonMaxSelectedPercent = MinSizeValue;

        private Form? attachedForm;
        private bool initialSizeApplied;
        private System.Windows.Forms.Timer? overlayHideTimer;
        private FormSysCommandHook? formSysCommandHook;

        private bool resizeOverlayActive;
        private bool inResizeLoop;
        private bool resizeChangedInLoop;

        private Size lastObservedSize = Size.Empty;
        private FormWindowState lastObservedWindowState = FormWindowState.Normal;

        [Browsable(true)]
        [DefaultValue(MinSizeValue)]
        public int SelectedPercent
        {
            get => selectedPercent;
            set
            {
                if (selectedPercent == value)
                    return;

                selectedPercent = value;

                if (!IsMaxSelectedValue(value))
                    lastNonMaxSelectedPercent = value;

                if (SyncDebugDisableStateAndReturnIsDisabled())
                    return;

                Invalidate();

                if (IsHandleCreated && attachedForm != null && attachedForm.IsHandleCreated && attachedForm.Visible)
                {
                    if (attachedForm.InvokeRequired)
                    {
                        attachedForm.BeginInvoke(new Action(() =>
                        {
                            if (SyncDebugDisableStateAndReturnIsDisabled())
                                return;

                            PrepareOverlayModeForCurrentSelection();
                            ShowResizeOverlay();
                            ApplyToParentForm();
                            RestartOverlayHideTimer();
                        }));
                    }
                    else
                    {
                        PrepareOverlayModeForCurrentSelection();
                        ShowResizeOverlay();
                        ApplyToParentForm();
                        RestartOverlayHideTimer();
                    }
                }
            }
        }

        public WindowSizePresetBar()
        {
            GetDebugDisableSnapshot(out debugDisableLatchedAtCreation, out debugDisableEpochAtCreation);

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", FontSizePx, FontStyle.Bold, GraphicsUnit.Point);
            Size = new Size(132, 28);
            Cursor = Cursors.Hand;

            if (ShouldThisInstanceBeDebugDisabled())
                ApplyDebugDisabledVisualState();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            AttachToParentFormIfNeeded();
            SyncDebugDisableStateAndReturnIsDisabled();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            AttachToParentFormIfNeeded();
            SyncDebugDisableStateAndReturnIsDisabled();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (overlayHideTimer != null)
                {
                    overlayHideTimer.Stop();
                    overlayHideTimer.Tick -= OverlayHideTimer_Tick;
                    overlayHideTimer.Dispose();
                    overlayHideTimer = null;
                }

                DetachFromForm();

                if (overlayForm != null)
                {
                    if (!overlayForm.IsDisposed)
                    {
                        overlayForm.Hide();
                        overlayForm.Close();
                        overlayForm.Dispose();
                    }

                    overlayForm = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle outer = new Rectangle(0, 0, Width - 1, Height - 1);

            using (GraphicsPath outerPath = CreateRoundRect(outer, CornerRadiusPx))
            using (SolidBrush outerBrush = new SolidBrush(Color.FromArgb(170, 12, 16, 24)))
            using (Pen outerPen = new Pen(Color.FromArgb(90, 255, 255, 255)))
            {
                e.Graphics.FillPath(outerBrush, outerPath);
                e.Graphics.DrawPath(outerPen, outerPath);
            }

            Rectangle[] segments = GetSegmentRects();

            for (int i = 0; i < segments.Length; i++)
            {
                bool isSelected = values[i] == selectedPercent;
                bool isHovered = i == hoveredIndex;
                bool isPressed = i == pressedIndex;

                Color fill =
                    isPressed ? Color.FromArgb(215, 82, 124, 186) :
                    isSelected ? Color.FromArgb(205, 74, 112, 170) :
                    isHovered ? Color.FromArgb(155, 58, 74, 96) :
                    Color.FromArgb(110, 32, 40, 54);

                Color border =
                    isSelected ? Color.FromArgb(170, 220, 235, 255) :
                    Color.FromArgb(80, 255, 255, 255);

                int segmentRadius = Math.Max(6, CornerRadiusPx - 3);

                using GraphicsPath segmentPath = CreateRoundRect(segments[i], segmentRadius);
                using SolidBrush fillBrush = new SolidBrush(fill);
                using Pen borderPen = new Pen(border);

                e.Graphics.FillPath(fillBrush, segmentPath);
                e.Graphics.DrawPath(borderPen, segmentPath);

                TextRenderer.DrawText(
                    e.Graphics,
                    GetDisplayTextForValue(values[i]),
                    Font,
                    segments[i],
                    ForeColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.EndEllipsis);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            int newIndex = HitTest(e.Location);
            if (newIndex != hoveredIndex)
            {
                hoveredIndex = newIndex;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            if (hoveredIndex != -1 || pressedIndex != -1)
            {
                hoveredIndex = -1;
                pressedIndex = -1;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            if (e.Button != MouseButtons.Left)
                return;

            pressedIndex = HitTest(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            if (e.Button != MouseButtons.Left)
                return;

            int hitIndex = HitTest(e.Location);
            int oldPressed = pressedIndex;
            pressedIndex = -1;

            if (hitIndex != -1 && hitIndex == oldPressed)
            {
                SelectedPercent = values[hitIndex];
                PrepareOverlayModeForCurrentSelection();
                ShowResizeOverlay();

                if (attachedForm != null)
                {
                    attachedForm.BeginInvoke(new Action(() =>
                    {
                        if (SyncDebugDisableStateAndReturnIsDisabled())
                            return;

                        ApplyToParentForm();
                        RestartOverlayHideTimer();
                    }));
                }
                else
                {
                    ApplyToParentForm();
                    RestartOverlayHideTimer();
                }
            }

            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            Invalidate();
        }

        private static void GetDebugDisableSnapshot(out bool isDisabled, out long epoch)
        {
            lock (DebugDisableSync)
            {
                isDisabled = debugDisable;
                epoch = debugDisableEpoch;
            }
        }

        private bool ShouldThisInstanceBeDebugDisabled()
        {
            if (!debugDisableLatchedAtCreation)
                return false;

            GetDebugDisableSnapshot(out bool isDisabled, out long epoch);
            return isDisabled && epoch == debugDisableEpochAtCreation;
        }

        private bool SyncDebugDisableStateAndReturnIsDisabled()
        {
            if (ShouldThisInstanceBeDebugDisabled())
            {
                ApplyDebugDisabledVisualState();
                return true;
            }

            RestoreFromDebugDisabledVisualStateIfNeeded();
            return false;
        }

        private bool CanStartNewOverlaySession()
        {
            return EnableOverlays;
        }

        private bool CanUseExistingOrNewOverlaySession()
        {
            return EnableOverlays || resizeOverlayActive || transitionOverlayActive;
        }

        private void ApplyDebugDisabledVisualState()
        {
            if (!debugDisableVisualStateApplied)
            {
                debugDisableRestoreVisible = base.Visible;
                debugDisableRestoreEnabled = base.Enabled;
                debugDisableVisualStateApplied = true;
            }

            hoveredIndex = -1;
            pressedIndex = -1;
            inResizeLoop = false;
            resizeChangedInLoop = false;

            HideResizeOverlay();

            if (base.Visible)
                base.Visible = false;

            if (base.Enabled)
                base.Enabled = false;
        }

        private void RestoreFromDebugDisabledVisualStateIfNeeded()
        {
            if (!debugDisableVisualStateApplied)
                return;

            base.Visible = debugDisableRestoreVisible;
            base.Enabled = debugDisableRestoreEnabled;
            debugDisableVisualStateApplied = false;

            if (Parent != null)
                Parent.PerformLayout();

            if (IsHandleCreated)
                Invalidate();
        }

        private void ApplyToParentForm()
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            Form? form = attachedForm ?? FindForm();
            if (form == null || !form.IsHandleCreated)
                return;

            form.SuspendLayout();

            try
            {
                form.StartPosition = FormStartPosition.Manual;

                if (IsMaxSelectedValue(selectedPercent))
                {
                    pendingMaximiseOverlay = true;

                    if (form.WindowState != FormWindowState.Maximized)
                        form.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    pendingMaximiseOverlay = false;

                    Rectangle area = Screen.FromRectangle(form.Bounds).WorkingArea;
                    double scale = selectedPercent / 100d;

                    int width = Math.Max(form.MinimumSize.Width > 0 ? form.MinimumSize.Width : 1, (int)Math.Round(area.Width * scale));
                    int height = Math.Max(form.MinimumSize.Height > 0 ? form.MinimumSize.Height : 1, (int)Math.Round(area.Height * scale));

                    width = Math.Min(width, area.Width);
                    height = Math.Min(height, area.Height);

                    Rectangle newBounds = new Rectangle(
                        area.Left + ((area.Width - width) / 2),
                        area.Top + ((area.Height - height) / 2),
                        width,
                        height);

                    if (form.WindowState != FormWindowState.Normal)
                        form.WindowState = FormWindowState.Normal;

                    form.Bounds = newBounds;
                }
            }
            finally
            {
                form.ResumeLayout(true);
                form.Invalidate(true);
                form.Update();
            }

            lastObservedSize = form.Size;
            lastObservedWindowState = form.WindowState;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size textA = TextRenderer.MeasureText(MinSizeValue.ToString()+"%", Font);
            Size textB = TextRenderer.MeasureText("75%", Font);
            Size textC = TextRenderer.MeasureText("Max", Font);

            int widest = Math.Max(textA.Width, Math.Max(textB.Width, textC.Width));
            int segmentWidth = widest + (HorizontalPaddingPx * 2);
            int width = (segmentWidth * 3) + (SegmentGapPx * 2) + 8;
            int height = Math.Max(28, textC.Height + 10);

            return new Size(width, height);
        }

        private void AttachToParentFormIfNeeded()
        {
            Form? form = FindForm();
            if (form == null || form == attachedForm)
                return;

            DetachFromForm();

            attachedForm = form;
            attachedForm.Shown += AttachedForm_Shown;
            attachedForm.ResizeBegin += AttachedForm_ResizeBegin;
            attachedForm.ResizeEnd += AttachedForm_ResizeEnd;
            attachedForm.SizeChanged += AttachedForm_SizeChanged;
            attachedForm.FormClosing += AttachedForm_FormClosing;
            attachedForm.FormClosed += AttachedForm_FormClosed;
            attachedForm.LocationChanged += AttachedForm_LocationChanged;
            attachedForm.HandleCreated += AttachedForm_HandleCreated;
            attachedForm.HandleDestroyed += AttachedForm_HandleDestroyed;

            formSysCommandHook = new FormSysCommandHook(this);
            AttachHookIfPossible();

            lastObservedSize = attachedForm.Size;
            lastObservedWindowState = attachedForm.WindowState;

            if (ShouldThisInstanceBeDebugDisabled())
            {
                ApplyDebugDisabledVisualState();
                return;
            }

            EnsureOverlayCreated();
        }

        private void DetachFromForm()
        {
            if (formSysCommandHook != null)
            {
                formSysCommandHook.Detach();
                formSysCommandHook = null;
            }

            if (attachedForm != null)
            {
                attachedForm.Shown -= AttachedForm_Shown;
                attachedForm.ResizeBegin -= AttachedForm_ResizeBegin;
                attachedForm.ResizeEnd -= AttachedForm_ResizeEnd;
                attachedForm.SizeChanged -= AttachedForm_SizeChanged;
                attachedForm.FormClosing -= AttachedForm_FormClosing;
                attachedForm.FormClosed -= AttachedForm_FormClosed;
                attachedForm.LocationChanged -= AttachedForm_LocationChanged;
                attachedForm.HandleCreated -= AttachedForm_HandleCreated;
                attachedForm.HandleDestroyed -= AttachedForm_HandleDestroyed;

                HideResizeOverlay();
            }

            attachedForm = null;
        }

        private void AttachedForm_HandleCreated(object? sender, EventArgs e)
        {
            AttachHookIfPossible();

            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            EnsureOverlayCreated();
        }

        private void AttachedForm_HandleDestroyed(object? sender, EventArgs e)
        {
            formSysCommandHook?.Detach();
        }

        private void AttachHookIfPossible()
        {
            if (attachedForm == null || !attachedForm.IsHandleCreated || formSysCommandHook == null)
                return;

            formSysCommandHook.Attach(attachedForm.Handle);
        }

        private void AttachedForm_Shown(object? sender, EventArgs e)
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            if (initialSizeApplied)
                return;

            initialSizeApplied = true;

            if (attachedForm == null)
                return;

            EnsureOverlayCreated();

            attachedForm.BeginInvoke(new Action(() =>
            {
                if (SyncDebugDisableStateAndReturnIsDisabled())
                    return;

                PrepareOverlayModeForCurrentSelection();
                ShowResizeOverlay();
                ApplyToParentForm();
                RestartOverlayHideTimer();
            }));
        }

        private void AttachedForm_ResizeBegin(object? sender, EventArgs e)
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
            {
                inResizeLoop = false;
                resizeChangedInLoop = false;
                return;
            }

            inResizeLoop = true;
            resizeChangedInLoop = false;
        }

        private void AttachedForm_ResizeEnd(object? sender, EventArgs e)
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
            {
                inResizeLoop = false;
                resizeChangedInLoop = false;
                HideResizeOverlay();
                return;
            }

            if (inResizeLoop)
            {
                inResizeLoop = false;

                if (resizeChangedInLoop)
                    RestartOverlayHideTimer();
                else
                    HideResizeOverlay();
            }
        }

        private void AttachedForm_SizeChanged(object? sender, EventArgs e)
        {
            if (attachedForm == null || attachedForm.IsDisposed || !attachedForm.IsHandleCreated)
                return;

            if (SyncDebugDisableStateAndReturnIsDisabled())
            {
                lastObservedSize = attachedForm.Size;
                lastObservedWindowState = attachedForm.WindowState;
                HideResizeOverlay();
                return;
            }

            Size previousSize = lastObservedSize;
            FormWindowState previousState = lastObservedWindowState;

            bool sizeChanged = attachedForm.Size != previousSize;
            bool stateChanged = attachedForm.WindowState != previousState;

            lastObservedSize = attachedForm.Size;
            lastObservedWindowState = attachedForm.WindowState;

            if (!sizeChanged && !stateChanged)
                return;

            if (attachedForm.WindowState == FormWindowState.Minimized)
            {
                HideResizeOverlay();
                return;
            }

            if (stateChanged)
            {
                if (attachedForm.WindowState == FormWindowState.Maximized)
                {
                    pendingMaximiseOverlay = true;
                    SetSelectedPercentWithoutApplying(MaxSelectedValue);
                }
                else if (attachedForm.WindowState == FormWindowState.Normal)
                {
                    pendingMaximiseOverlay = false;

                    if (previousState == FormWindowState.Maximized && selectedPercent == MaxSelectedValue)
                        SetSelectedPercentWithoutApplying(lastNonMaxSelectedPercent);
                }
            }

            transitionOverlayActive = false;
            overlayPreviewBounds = null;

            if (resizeOverlayActive && overlayForm != null && !overlayForm.IsDisposed)
            {
                UpdateOverlayBounds();
                BringOverlayToFront();
            }

            if (inResizeLoop)
            {
                resizeChangedInLoop = true;
                ShowResizeOverlay();
                return;
            }

            ShowResizeOverlay();
            RestartOverlayHideTimer();
        }

        private void AttachedForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            HideResizeOverlay();
        }

        private void AttachedForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            DetachFromForm();
        }

        private void AttachedForm_LocationChanged(object? sender, EventArgs e)
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
            {
                HideResizeOverlay();
                return;
            }

            if (attachedForm == null || overlayForm == null || overlayForm.IsDisposed || !resizeOverlayActive)
                return;

            if (transitionOverlayActive)
                return;

            UpdateOverlayBounds();
        }

        private void HandleAttachedFormPreTransitionFromNcAction(int command)
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            if (attachedForm == null || attachedForm.IsDisposed || !attachedForm.IsHandleCreated)
                return;

            if (!attachedForm.Visible)
                return;

            ForceShowTransitionOverlay(command, aggressivePaint: true);
        }

        private void HandleAttachedFormSysCommand(int command)
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            if (attachedForm == null || attachedForm.IsDisposed || !attachedForm.IsHandleCreated)
                return;

            if (!attachedForm.Visible)
                return;

            command &= 0xFFF0;

            if (command == SC_MAXIMIZE)
            {
                if (attachedForm.WindowState == FormWindowState.Maximized)
                    return;

                ForceShowTransitionOverlay(command, aggressivePaint: true);
                return;
            }

            if (command == SC_RESTORE)
            {
                if (attachedForm.WindowState == FormWindowState.Normal)
                    return;

                ForceShowTransitionOverlay(command, aggressivePaint: true);
            }
        }

        private void ForceShowTransitionOverlay(int command, bool aggressivePaint)
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            if (!CanStartNewOverlaySession())
                return;

            if (attachedForm == null)
                return;

            bool wantsMaximise = (command & 0xFFF0) == SC_MAXIMIZE;

            if (transitionOverlayActive && pendingMaximiseOverlay == wantsMaximise)
            {
                RestartOverlayHideTimer();
                return;
            }

            EnsureOverlayCreated();
            if (overlayForm == null)
                return;

            BeginOverlayRenderSessionIfNeeded();

            pendingMaximiseOverlay = wantsMaximise;
            transitionOverlayActive = true;
            overlayPreviewBounds = GetBruteForceTransitionBounds(command);

            overlayForm.Bounds = overlayPreviewBounds.Value;
            ShowOverlayImmediately();

            if (aggressivePaint)
                ForceOverlayToRenderNow();

            RestartOverlayHideTimer();
            resizeOverlayActive = true;
        }

        private Rectangle GetBruteForceTransitionBounds(int command)
        {
            if (attachedForm == null)
                return Rectangle.Empty;

            Rectangle referenceBounds = attachedForm.Bounds;

            if ((command & 0xFFF0) == SC_RESTORE)
            {
                Rectangle restoreBounds = attachedForm.RestoreBounds;
                if (restoreBounds.Width > 0 && restoreBounds.Height > 0)
                    referenceBounds = restoreBounds;
            }

            Screen screen = Screen.FromRectangle(referenceBounds);
            bool maximising = (command & 0xFFF0) == SC_MAXIMIZE;

            int topReservedScreenPx = EstimateTopReservedScreenHeightForTransition(screen, maximising);

            if (maximising)
                topReservedScreenPx = Math.Max(0, topReservedScreenPx - MaximisePreviewTopCorrectionPx);

            return TrimTop(screen.Bounds, topReservedScreenPx);
        }

        private void EnsureOverlayCreated()
        {
            if (overlayForm != null && !overlayForm.IsDisposed)
            {
                if (attachedForm != null && overlayForm.Owner != attachedForm)
                    overlayForm.Owner = attachedForm;

                return;
            }

            if (!CanUseExistingOrNewOverlaySession())
                return;

            overlayForm = new ResizeOverlayForm();

            if (attachedForm != null)
                overlayForm.Owner = attachedForm;

            _ = overlayForm.Handle;
        }

        private void UpdateOverlayBounds()
        {
            if (attachedForm == null || overlayForm == null || overlayForm.IsDisposed)
                return;

            Rectangle bounds;

            if (transitionOverlayActive && overlayPreviewBounds.HasValue)
            {
                bounds = overlayPreviewBounds.Value;
            }
            else if (pendingMaximiseOverlay)
            {
                Rectangle area = Screen.FromRectangle(attachedForm.Bounds).WorkingArea;
                int topOffset = HasWindowChrome(attachedForm) ? EstimateTopChromeHeight(attachedForm) : 0;
                int topReservedClient = EstimateTopReservedClientHeight(attachedForm);

                int topTrim = Math.Max(0, topOffset + topReservedClient - MaximisePreviewTopCorrectionPx);

                bounds = new Rectangle(
                    area.Left,
                    area.Top + topTrim,
                    area.Width,
                    Math.Max(1, area.Height - topTrim));
            }
            else
            {
                Rectangle clientBounds = attachedForm.RectangleToScreen(attachedForm.ClientRectangle);
                int topReservedClient = EstimateTopReservedClientHeight(attachedForm);
                bounds = TrimTop(clientBounds, topReservedClient);
            }

            overlayForm.Bounds = bounds;
            ApplyOverlayDebugRenderState();
            overlayForm.PerformLayout();
            overlayForm.Invalidate();
            overlayForm.Update();
        }
        private static bool HasWindowChrome(Form form)
        {
            return form.FormBorderStyle != FormBorderStyle.None;
        }

        private static int EstimateTopChromeHeight(Form form)
        {
            Rectangle clientOnScreen = form.RectangleToScreen(form.ClientRectangle);
            return Math.Max(0, clientOnScreen.Top - form.Bounds.Top);
        }

        private int EstimateTopReservedScreenHeightForTransition(Screen screen, bool maximising)
        {
            if (attachedForm == null)
                return 0;

            int chromeHeight = HasWindowChrome(attachedForm) ? EstimateTopChromeHeight(attachedForm) : 0;
            int topReservedClient = EstimateTopReservedClientHeight(attachedForm);

            if (maximising)
                return Math.Max(0, chromeHeight + topReservedClient);

            return Math.Max(0, chromeHeight + topReservedClient);
        }

        private int EstimateTopReservedClientHeight(Form form)
        {
            if (form.ClientSize.Width <= 0 || form.ClientSize.Height <= 0)
                return 0;

            int widestAcceptableMinWidth = (int)Math.Round(form.ClientSize.Width * (TopReservedMinWidthPercent / 100d));
            int maxBottom = 0;

            foreach (Control child in form.Controls)
                maxBottom = Math.Max(maxBottom, GetTopReservedBottomRecursive(child, widestAcceptableMinWidth));

            if (maxBottom <= 0)
                return 0;

            return Math.Min(form.ClientSize.Height - 1, maxBottom + TopReservedExtraPadPx);
        }

        private int GetTopReservedBottomRecursive(Control control, int widestAcceptableMinWidth)
        {
            if (!IsLikelyTopReservedControl(control, widestAcceptableMinWidth))
                return 0;

            int bottom = control.Bottom;

            foreach (Control child in control.Controls)
                bottom = Math.Max(bottom, control.Top + GetTopReservedBottomRecursive(child, widestAcceptableMinWidth));

            return bottom;
        }

        private bool IsLikelyTopReservedControl(Control control, int widestAcceptableMinWidth)
        {
            if (!control.Visible)
                return false;

            if (control.Width <= 0 || control.Height <= 0)
                return false;

            if (control.Bottom <= 0)
                return false;

            if (control.Top > TopReservedLooseTopThresholdPx)
                return false;

            if (control.Dock == DockStyle.Fill)
                return false;

            bool isMenuLike =
                control is MenuStrip ||
                control is ToolStrip ||
                control is ToolStripContainer;

            bool isDockedTop = control.Dock == DockStyle.Top;
            bool spansEnoughWidth = control.Width >= widestAcceptableMinWidth;
            bool sitsNearTop = control.Top <= TopReservedLooseTopThresholdPx;

            if (isMenuLike && sitsNearTop)
                return true;

            if (isDockedTop && spansEnoughWidth)
                return true;

            if (sitsNearTop && spansEnoughWidth)
            {
                bool likelyHeaderLike =
                    control is Panel ||
                    control is UserControl ||
                    control is FlowLayoutPanel ||
                    control is TableLayoutPanel;

                if (likelyHeaderLike)
                    return true;
            }

            return false;
        }

        private static Rectangle TrimTop(Rectangle rect, int trimPx)
        {
            int safeTrim = Math.Max(0, Math.Min(trimPx, rect.Height - 1));
            return new Rectangle(
                rect.Left,
                rect.Top + safeTrim,
                rect.Width,
                Math.Max(1, rect.Height - safeTrim));
        }

        private void OverlayHideTimer_Tick(object? sender, EventArgs e)
        {
            if (overlayHideTimer == null)
                return;

            overlayHideTimer.Stop();
            HideResizeOverlay();
        }

        private void RestartOverlayHideTimer()
        {
            if (ShouldThisInstanceBeDebugDisabled())
                return;

            if (!CanUseExistingOrNewOverlaySession())
                return;

            overlayHideTimer ??= CreateOverlayHideTimer();
            overlayHideTimer.Stop();
            overlayHideTimer.Start();
        }

        private System.Windows.Forms.Timer CreateOverlayHideTimer()
        {
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = OverlayHideDelayMs;
            timer.Tick += OverlayHideTimer_Tick;
            return timer;
        }

        private void BeginOverlayRenderSessionIfNeeded()
        {
            if (overlayForm == null || overlayForm.IsDisposed)
                return;

            if (!resizeOverlayActive || overlayRenderPrimitiveValue <= 0)
                overlayRenderPrimitiveValue = Interlocked.Increment(ref overlayRenderPrimitiveCounter);

            ApplyOverlayDebugRenderState();
        }

        private void ApplyOverlayDebugRenderState()
        {
            if (overlayForm == null || overlayForm.IsDisposed)
                return;

            overlayForm.SetDebugRender(DebugRender && overlayRenderPrimitiveValue > 0, overlayRenderPrimitiveValue);
        }

        private void ShowResizeOverlay(bool usePreviewBounds = false)
        {
            if (SyncDebugDisableStateAndReturnIsDisabled())
                return;

            if (!CanUseExistingOrNewOverlaySession())
                return;

            if (attachedForm == null || !attachedForm.IsHandleCreated || attachedForm.IsDisposed)
                return;

            if (!attachedForm.Visible)
                return;

            EnsureOverlayCreated();
            if (overlayForm == null)
                return;

            if (usePreviewBounds && overlayPreviewBounds.HasValue)
                overlayForm.Bounds = overlayPreviewBounds.Value;
            else
                UpdateOverlayBounds();

            BeginOverlayRenderSessionIfNeeded();
            ShowOverlayImmediately();
            resizeOverlayActive = true;
        }

        private void ShowOverlayImmediately()
        {
            if (overlayForm == null || overlayForm.IsDisposed)
                return;

            IntPtr handle = overlayForm.Handle;

            NativeMethods.ShowWindow(handle, NativeMethods.SW_SHOWNOACTIVATE);
            NativeMethods.SetWindowPos(
                handle,
                NativeMethods.HWND_TOP,
                overlayForm.Left,
                overlayForm.Top,
                overlayForm.Width,
                overlayForm.Height,
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
            NativeMethods.UpdateWindow(handle);

            overlayForm.PerformLayout();
            overlayForm.Invalidate();
            overlayForm.Update();
        }

        private void ForceOverlayToRenderNow()
        {
            if (overlayForm == null || overlayForm.IsDisposed || !overlayForm.IsHandleCreated)
                return;

            if (overlayPaintPumpActive)
                return;

            overlayPaintPumpActive = true;

            try
            {
                ApplyOverlayDebugRenderState();

                IntPtr handle = overlayForm.Handle;

                overlayForm.PerformLayout();
                overlayForm.Invalidate(true);
                overlayForm.Update();
                overlayForm.Refresh();

                NativeMethods.RedrawWindow(
                    handle,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    NativeMethods.RDW_INVALIDATE |
                    NativeMethods.RDW_ERASE |
                    NativeMethods.RDW_ALLCHILDREN |
                    NativeMethods.RDW_UPDATENOW);

                NativeMethods.UpdateWindow(handle);

                try
                {
                    Application.DoEvents();
                }
                catch
                {
                }

                try
                {
                    NativeMethods.DwmFlush();
                }
                catch
                {
                }
            }
            finally
            {
                overlayPaintPumpActive = false;
            }
        }

        private void BringOverlayToFront()
        {
            if (overlayForm == null || overlayForm.IsDisposed)
                return;

            NativeMethods.SetWindowPos(
                overlayForm.Handle,
                NativeMethods.HWND_TOP,
                overlayForm.Left,
                overlayForm.Top,
                overlayForm.Width,
                overlayForm.Height,
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
        }

        private void HideResizeOverlay()
        {
            if (overlayHideTimer != null)
                overlayHideTimer.Stop();

            if (overlayForm != null && !overlayForm.IsDisposed && overlayForm.IsHandleCreated)
                NativeMethods.ShowWindow(overlayForm.Handle, NativeMethods.SW_HIDE);

            overlayPreviewBounds = null;
            transitionOverlayActive = false;
            pendingMaximiseOverlay = false;
            resizeOverlayActive = false;
            overlayRenderPrimitiveValue = 0;

            if (attachedForm != null && !attachedForm.IsDisposed)
            {
                attachedForm.Invalidate(true);
                attachedForm.Update();
            }
        }

        private int HitTest(Point point)
        {
            Rectangle[] segments = GetSegmentRects();

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Contains(point))
                    return i;
            }

            return -1;
        }

        private Rectangle[] GetSegmentRects()
        {
            int innerX = 4;
            int innerY = 4;
            int innerWidth = Width - 8;
            int innerHeight = Height - 8;

            int totalGap = SegmentGapPx * (values.Length - 1);
            int segmentWidth = (innerWidth - totalGap) / values.Length;

            Rectangle[] result = new Rectangle[values.Length];

            int x = innerX;
            for (int i = 0; i < values.Length; i++)
            {
                int width = i == values.Length - 1
                    ? (innerX + innerWidth) - x
                    : segmentWidth;

                result[i] = new Rectangle(x, innerY, width, innerHeight);
                x += width + SegmentGapPx;
            }

            return result;
        }

        private void PrepareOverlayModeForCurrentSelection()
        {
            transitionOverlayActive = false;
            overlayPreviewBounds = null;
            pendingMaximiseOverlay = IsMaxSelectedValue(selectedPercent);
        }

        private void SetSelectedPercentWithoutApplying(int value)
        {
            if (selectedPercent == value)
                return;

            selectedPercent = value;

            if (!IsMaxSelectedValue(value))
                lastNonMaxSelectedPercent = value;

            if (!SyncDebugDisableStateAndReturnIsDisabled() && IsHandleCreated)
                Invalidate();
        }

        private static bool IsMaxSelectedValue(int value)
        {
            return value == MaxSelectedValue;
        }

        private static string GetDisplayTextForValue(int value)
        {
            return IsMaxSelectedValue(value)
                ? "Max"
                : value.ToString(CultureInfo.InvariantCulture) + "%";
        }

        private static GraphicsPath CreateRoundRect(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        internal void Dispose_()
        {
            Dispose();
        }

        private sealed class FormSysCommandHook : NativeWindow
        {
            private readonly WindowSizePresetBar owner;
            private IntPtr attachedHandle;

            public FormSysCommandHook(WindowSizePresetBar owner)
            {
                this.owner = owner;
            }

            public void Attach(IntPtr handle)
            {
                if (handle == IntPtr.Zero)
                    return;

                if (attachedHandle == handle)
                    return;

                Detach();
                AssignHandle(handle);
                attachedHandle = handle;
            }

            public void Detach()
            {
                if (Handle != IntPtr.Zero)
                    ReleaseHandle();

                attachedHandle = IntPtr.Zero;
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_NCLBUTTONDOWN:
                        {
                            int hit = unchecked((int)(long)m.WParam) & 0xFFFF;
                            if (hit == HTMAXBUTTON)
                            {
                                int command =
                                    owner.attachedForm?.WindowState == FormWindowState.Maximized
                                        ? SC_RESTORE
                                        : SC_MAXIMIZE;

                                owner.HandleAttachedFormPreTransitionFromNcAction(command);
                            }
                            break;
                        }

                    case WM_NCLBUTTONDBLCLK:
                        {
                            int hit = unchecked((int)(long)m.WParam) & 0xFFFF;
                            if (hit == HTCAPTION)
                            {
                                int command =
                                    owner.attachedForm?.WindowState == FormWindowState.Maximized
                                        ? SC_RESTORE
                                        : SC_MAXIMIZE;

                                owner.HandleAttachedFormPreTransitionFromNcAction(command);
                            }
                            break;
                        }

                    case WM_SYSCOMMAND:
                        {
                            int command = m.WParam.ToInt32() & 0xFFF0;
                            if (command == SC_MAXIMIZE || command == SC_RESTORE)
                                owner.HandleAttachedFormSysCommand(command);
                            break;
                        }
                }

                base.WndProc(ref m);
            }
        }

        private static class NativeMethods
        {
            public static readonly IntPtr HWND_TOP = IntPtr.Zero;
            public static readonly IntPtr HWND_TOPMOST = new(-1);

            public const uint SWP_NOSIZE = 0x0001;
            public const uint SWP_NOMOVE = 0x0002;
            public const uint SWP_NOACTIVATE = 0x0010;
            public const uint SWP_SHOWWINDOW = 0x0040;

            public const int SW_HIDE = 0;
            public const int SW_SHOWNOACTIVATE = 4;

            public const uint RDW_INVALIDATE = 0x0001;
            public const uint RDW_ERASE = 0x0004;
            public const uint RDW_ALLCHILDREN = 0x0080;
            public const uint RDW_UPDATENOW = 0x0100;

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetWindowPos(
                IntPtr hWnd,
                IntPtr hWndInsertAfter,
                int X,
                int Y,
                int cx,
                int cy,
                uint uFlags);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UpdateWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RedrawWindow(
                IntPtr hWnd,
                IntPtr lprcUpdate,
                IntPtr hrgnUpdate,
                uint flags);

            [DllImport("dwmapi.dll")]
            public static extern int DwmFlush();
        }

        private sealed class ResizeOverlayForm : Form
        {
            private const int WS_EX_NOACTIVATE = 0x08000000;
            private const int WS_EX_TOOLWINDOW = 0x00000080;

            private static readonly MemoryStream SpinnerStream = new(Resources.Loading_icon);
            private static readonly Image SpinnerImage = Image.FromStream(SpinnerStream);

            private const int CardWidthPx = 280;
            private const int CardHeightPx = 148;
            private const int DebugCardMinHeightPx = 208;
            private const int CardCornerRadiusPx = 20;
            private const int CardShadowOffsetPx = 7;

            private const int SpinnerSizePx = 56;
            private const int SpinnerTopInsetPx = 22;
            private const int SpinnerTilePadPx = 10;
            private const int SpinnerTileCornerRadiusPx = 14;

            private const int TextGapPx = 14;
            private const int DebugPrimitiveGapPx = 10;
            private const int DebugPrimitiveSideInsetPx = 18;
            private const int DebugPrimitiveBottomInsetPx = 18;

            private readonly PictureBox spinnerOverlay;
            private readonly Label resizingLabel;
            private readonly Label debugPrimitiveLabel;

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
                    return cp;
                }
            }

            public ResizeOverlayForm()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                TopMost = false;
                BackColor = Color.FromArgb(238, 241, 246);
                Opacity = 1d;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw,
                    true);

                spinnerOverlay = new PictureBox
                {
                    Size = new Size(SpinnerSizePx, SpinnerSizePx),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = SpinnerImage,
                    BackColor = Color.Transparent,
                    TabStop = false
                };

                resizingLabel = new Label
                {
                    AutoSize = true,
                    Text = "Rendering...",
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(28, 28, 28),
                    Font = new Font("Segoe UI Semibold", 12f, FontStyle.Regular, GraphicsUnit.Point)
                };

                debugPrimitiveLabel = new Label
                {
                    AutoSize = false,
                    Visible = false,
                    Text = string.Empty,
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(240, 24, 24),
                    Font = new Font("Consolas", 22f, FontStyle.Bold, GraphicsUnit.Point),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                Controls.Add(spinnerOverlay);
                Controls.Add(resizingLabel);
                Controls.Add(debugPrimitiveLabel);

                LayoutOverlay();
            }

            protected override bool ShowWithoutActivation => true;

            public void SetDebugRender(bool enabled, long primitiveValue)
            {
                debugPrimitiveLabel.Visible = enabled;
                debugPrimitiveLabel.Text = enabled
                    ? primitiveValue.ToString(CultureInfo.InvariantCulture)
                    : string.Empty;

                LayoutOverlay();
                Invalidate();
            }

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                LayoutOverlay();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                LayoutOverlay();
                Invalidate();
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                base.OnLayout(levent);
                LayoutOverlay();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle card = GetCardBounds();
                Rectangle shadowRect = new Rectangle(card.X, card.Y + CardShadowOffsetPx, card.Width, card.Height);

                using (GraphicsPath shadowPath = CreateRoundedPath(shadowRect, CardCornerRadiusPx))
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(26, 0, 0, 0)))
                {
                    e.Graphics.FillPath(shadowBrush, shadowPath);
                }

                using (GraphicsPath cardPath = CreateRoundedPath(card, CardCornerRadiusPx))
                using (SolidBrush cardBrush = new SolidBrush(Color.FromArgb(252, 252, 253)))
                using (Pen cardBorderPen = new Pen(Color.FromArgb(38, 0, 0, 0)))
                {
                    e.Graphics.FillPath(cardBrush, cardPath);
                    e.Graphics.DrawPath(cardBorderPen, cardPath);
                }

                Rectangle spinnerTile = GetSpinnerTileBounds();

                using (GraphicsPath tilePath = CreateRoundedPath(spinnerTile, SpinnerTileCornerRadiusPx))
                using (SolidBrush tileBrush = new SolidBrush(Color.FromArgb(20, 22, 26)))
                using (Pen tileBorderPen = new Pen(Color.FromArgb(55, 255, 255, 255)))
                {
                    e.Graphics.FillPath(tileBrush, tilePath);
                    e.Graphics.DrawPath(tileBorderPen, tilePath);
                }
            }

            private void LayoutOverlay()
            {
                if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                    return;

                Rectangle card = GetCardBounds();

                spinnerOverlay.Location = new Point(
                    card.Left + ((card.Width - spinnerOverlay.Width) / 2),
                    card.Top + SpinnerTopInsetPx);

                resizingLabel.Location = new Point(
                    card.Left + ((card.Width - resizingLabel.Width) / 2),
                    spinnerOverlay.Bottom + TextGapPx);

                if (debugPrimitiveLabel.Visible)
                {
                    int availableWidth = Math.Max(1, card.Width - (DebugPrimitiveSideInsetPx * 2));
                    Size debugSize = debugPrimitiveLabel.GetPreferredSize(new Size(availableWidth, 0));

                    debugPrimitiveLabel.Bounds = new Rectangle(
                        card.Left + ((card.Width - Math.Min(availableWidth, debugSize.Width)) / 2),
                        resizingLabel.Bottom + DebugPrimitiveGapPx,
                        Math.Min(availableWidth, debugSize.Width),
                        debugSize.Height);
                }
                else
                {
                    debugPrimitiveLabel.Bounds = Rectangle.Empty;
                }
            }

            private Rectangle GetCardBounds()
            {
                Size cardSize = GetCardSize();

                return new Rectangle(
                    (ClientSize.Width - cardSize.Width) / 2,
                    (ClientSize.Height - cardSize.Height) / 2,
                    cardSize.Width,
                    cardSize.Height);
            }

            private Size GetCardSize()
            {
                int width = CardWidthPx;
                int height = CardHeightPx;

                if (debugPrimitiveLabel.Visible && !string.IsNullOrWhiteSpace(debugPrimitiveLabel.Text))
                {
                    Size rawTextSize = TextRenderer.MeasureText(debugPrimitiveLabel.Text, debugPrimitiveLabel.Font);
                    width = Math.Max(width, rawTextSize.Width + (DebugPrimitiveSideInsetPx * 2) + 10);
                    height = Math.Max(height, DebugCardMinHeightPx);

                    int availableWidth = Math.Max(1, width - (DebugPrimitiveSideInsetPx * 2));
                    Size debugSize = debugPrimitiveLabel.GetPreferredSize(new Size(availableWidth, 0));
                    height = Math.Max(
                        height,
                        SpinnerTopInsetPx +
                        spinnerOverlay.Height +
                        TextGapPx +
                        resizingLabel.Height +
                        DebugPrimitiveGapPx +
                        debugSize.Height +
                        DebugPrimitiveBottomInsetPx);
                }

                return new Size(width, height);
            }

            private Rectangle GetSpinnerTileBounds()
            {
                return new Rectangle(
                    spinnerOverlay.Left - SpinnerTilePadPx,
                    spinnerOverlay.Top - SpinnerTilePadPx,
                    spinnerOverlay.Width + (SpinnerTilePadPx * 2),
                    spinnerOverlay.Height + (SpinnerTilePadPx * 2));
            }

            private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
            {
                int diameter = radius * 2;
                GraphicsPath path = new GraphicsPath();

                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                return path;
            }
        }
    }
}