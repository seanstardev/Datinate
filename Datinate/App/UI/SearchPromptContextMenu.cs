namespace datinate.app
{
    public sealed class SearchPromptContextMenu : ContextMenuStrip, IMessageFilter
    {
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int WM_POINTERWHEEL = 0x024E;
        private const int WM_POINTERHWHEEL = 0x024F;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        // NOTE: Only one prompt menu should ever be visible application-wide.
        private static SearchPromptContextMenu? activeMenu;

        private Control? anchorControl;
        private Form? ownerForm;

        private Point anchorScreenLocation;
        private Size anchorSize;

        private bool messageFilterInstalled;

        private readonly System.Windows.Forms.Timer monitorTimer;

        public SearchPromptContextMenu()
        {
            AutoClose = false;

            monitorTimer = new System.Windows.Forms.Timer
            {
                Interval = 50
            };

            monitorTimer.Tick += MonitorTimer_Tick;
        }

        public void ShowFor(Control control)
        {
            // NOTE: Only one search prompt menu may be visible at a time.
            if (activeMenu != null && !ReferenceEquals(activeMenu, this))
                activeMenu.Close();

            Close();
            Detach();

            activeMenu = this;

            anchorControl = control;
            ownerForm = control.FindForm();

            anchorControl.Leave += AnchorControl_Leave;
            anchorControl.Disposed += AnchorControl_Disposed;

            if (ownerForm != null)
            {
                ownerForm.Deactivate += OwnerForm_Deactivate;
                ownerForm.FormClosed += OwnerForm_FormClosed;
            }

            Application.AddMessageFilter(this);
            messageFilterInstalled = true;

            Show(
                control,
                new Point(0, control.Height));

            // NOTE: Menu must not steal keyboard input from the filter.
            control.Focus();

            anchorScreenLocation = control.PointToScreen(Point.Empty);
            anchorSize = control.Size;

            monitorTimer.Start();
        }

        public bool PreFilterMessage(ref Message m)
        {
            // NOTE: Escape globally dismisses the active prompt menu.
            if ((m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN) &&
                (Keys)(int)m.WParam == Keys.Escape)
            {
                Close();

                // NOTE: Consume Escape so it does not also trigger something elsewhere.
                return true;
            }

            switch (m.Msg)
            {
                case WM_MOUSEWHEEL:
                case WM_MOUSEHWHEEL:
                case WM_POINTERWHEEL:
                case WM_POINTERHWHEEL:
                    Close();
                    break;
            }

            // NOTE: Observe scrolling only. Never consume it.
            return false;
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            Detach();

            if (ReferenceEquals(activeMenu, this))
                activeMenu = null;

            base.OnClosed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Detach();
                monitorTimer.Dispose();

                if (ReferenceEquals(activeMenu, this))
                    activeMenu = null;
            }

            base.Dispose(disposing);
        }

        private void MonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (anchorControl == null || anchorControl.IsDisposed)
            {
                Close();
                return;
            }

            // NOTE: Covers hidden controls and switching away from the TabPage
            // containing the search field.
            if (!IsActuallyVisible(anchorControl))
            {
                Close();
                return;
            }

            if (ownerForm == null ||
                ownerForm.IsDisposed ||
                !ownerForm.Visible ||
                ownerForm.WindowState == FormWindowState.Minimized)
            {
                Close();
                return;
            }

            Point currentLocation = anchorControl.PointToScreen(Point.Empty);

            // NOTE: Scrolling, layout changes, or ancestor movement that moves
            // the textbox makes the popup's absolute screen position stale.
            if (currentLocation != anchorScreenLocation ||
                anchorControl.Size != anchorSize)
            {
                Close();
            }
        }

        private static bool IsActuallyVisible(Control control)
        {
            Control? current = control;

            while (current != null)
            {
                if (!current.Visible)
                    return false;

                // NOTE: TabPage.Visible is not sufficient to determine whether
                // it is currently the selected page.
                if (current is TabPage page &&
                    page.Parent is TabControl tabControl &&
                    tabControl.SelectedTab != page)
                {
                    return false;
                }

                current = current.Parent;
            }

            return true;
        }

        private void AnchorControl_Leave(object? sender, EventArgs e)
        {
            if (anchorControl == null || anchorControl.IsDisposed)
                return;

            anchorControl.BeginInvoke(new Action(() =>
            {
                if (anchorControl != null &&
                    !anchorControl.IsDisposed &&
                    !anchorControl.Focused &&
                    !ContainsFocus)
                {
                    Close();
                }
            }));
        }

        private void AnchorControl_Disposed(object? sender, EventArgs e)
        {
            Close();
        }

        private void OwnerForm_Deactivate(object? sender, EventArgs e)
        {
            Close();
        }

        private void OwnerForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void Detach()
        {
            monitorTimer.Stop();

            if (anchorControl != null)
            {
                anchorControl.Leave -= AnchorControl_Leave;
                anchorControl.Disposed -= AnchorControl_Disposed;
                anchorControl = null;
            }

            if (ownerForm != null)
            {
                ownerForm.Deactivate -= OwnerForm_Deactivate;
                ownerForm.FormClosed -= OwnerForm_FormClosed;
                ownerForm = null;
            }

            if (messageFilterInstalled)
            {
                Application.RemoveMessageFilter(this);
                messageFilterInstalled = false;
            }
        }
    }
}
