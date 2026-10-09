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

        private IReadOnlyList<string> prompts = Array.Empty<string>();

        private TextBoxBase? textBox;
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

        /// <summary>
        /// Attaches this prompt menu to a text field.
        /// </summary>
        public void Attach(TextBoxBase textBox)
        {
            if (ReferenceEquals(this.textBox, textBox))
                return;

            DetachTextBox();

            this.textBox = textBox;

            textBox.Click += TextBox_Click;
            textBox.TextChanged += TextBox_TextChanged;
            textBox.Leave += TextBox_Leave;
            textBox.Disposed += TextBox_Disposed;
        }

        public void SetPrompts(IReadOnlyList<string>? prompts)
        {
            this.prompts = prompts ?? Array.Empty<string>();

            Close();
        }

        public void ShowIfApplicable()
        {
            if (textBox == null || textBox.IsDisposed)
                return;

            // NOTE: Prompts are only offered as shortcuts when the field is empty.
            if (textBox.TextLength != 0)
                return;

            if (prompts.Count == 0)
                return;

            ShowPrompts();
        }

        private void ShowPrompts()
        {
            if (textBox == null)
                return;

            // NOTE: Only one search prompt menu may be visible at a time.
            if (activeMenu != null && !ReferenceEquals(activeMenu, this))
                activeMenu.Close();

            Close();
            DetachActiveState();

            activeMenu = this;

            Items.Clear();

            foreach (var prompt in prompts)
            {
                var item = new ToolStripMenuItem(prompt);

                item.Click += (_, _) =>
                {
                    if (textBox == null || textBox.IsDisposed)
                        return;

                    textBox.Text = prompt;
                    textBox.SelectionStart = textBox.TextLength;
                    textBox.Focus();
                };

                Items.Add(item);
            }

            ownerForm = textBox.FindForm();

            if (ownerForm != null)
            {
                ownerForm.Deactivate += OwnerForm_Deactivate;
                ownerForm.FormClosed += OwnerForm_FormClosed;
            }

            Application.AddMessageFilter(this);
            messageFilterInstalled = true;

            ShowAtBestLocation();

            // NOTE: Menu must not steal keyboard input from the text field.
            textBox.Focus();

            anchorScreenLocation = textBox.PointToScreen(Point.Empty);
            anchorSize = textBox.Size;

            monitorTimer.Start();
        }

        public bool PreFilterMessage(ref Message m)
        {
            // NOTE: Escape globally dismisses the active prompt menu.
            if ((m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN) &&
                (Keys)(int)m.WParam == Keys.Escape)
            {
                Close();
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
            DetachActiveState();

            if (ReferenceEquals(activeMenu, this))
                activeMenu = null;

            base.OnClosed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachActiveState();
                DetachTextBox();

                monitorTimer.Dispose();

                if (ReferenceEquals(activeMenu, this))
                    activeMenu = null;
            }

            base.Dispose(disposing);
        }
        private void ShowAtBestLocation()
        {
            if (textBox == null)
                return;

            const int popupGap = 2;

            Size menuSize = GetPreferredSize(Size.Empty);

            Point textBoxScreenLocation =
                textBox.PointToScreen(Point.Empty);

            Rectangle textBoxBounds = new Rectangle(
                textBoxScreenLocation,
                textBox.Size);

            Rectangle workingArea =
                Screen.FromControl(textBox).WorkingArea;

            int spaceBelow =
                workingArea.Bottom - textBoxBounds.Bottom;

            int spaceAbove =
                textBoxBounds.Top - workingArea.Top;

            // NOTE:
            // Prefer below when it fits. If it does not fit, use above when possible.
            // If neither side can fully contain the menu, use whichever has more room.
            bool showBelow =
                menuSize.Height + popupGap <= spaceBelow ||
                spaceBelow >= spaceAbove;

            if (showBelow)
            {
                Show(
                    textBox,
                    new Point(0, textBox.Height + popupGap),
                    ToolStripDropDownDirection.BelowRight);
            }
            else
            {
                Show(
                    textBox,
                    new Point(0, -popupGap),
                    ToolStripDropDownDirection.AboveRight);
            }
        }
        private void TextBox_Click(object? sender, EventArgs e)
        {
            ShowIfApplicable();
        }

        private void TextBox_TextChanged(object? sender, EventArgs e)
        {
            if (textBox?.TextLength > 0)
                Close();
        }

        private void TextBox_Leave(object? sender, EventArgs e)
        {
            if (textBox == null || textBox.IsDisposed)
                return;

            textBox.BeginInvoke(new Action(() =>
            {
                if (textBox != null &&
                    !textBox.IsDisposed &&
                    !textBox.Focused &&
                    !ContainsFocus)
                {
                    Close();
                }
            }));
        }

        private void TextBox_Disposed(object? sender, EventArgs e)
        {
            // NOTE: The menu belongs to the attached text field.
            Dispose();
        }

        private void MonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (textBox == null || textBox.IsDisposed)
            {
                Close();
                return;
            }

            // NOTE: Covers hidden controls and switching away from the TabPage
            // containing the text field.
            if (!IsActuallyVisible(textBox))
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

            Point currentLocation = textBox.PointToScreen(Point.Empty);

            // NOTE: Scrolling, layout changes, or ancestor movement that moves
            // the text field makes the popup's screen position stale.
            if (currentLocation != anchorScreenLocation ||
                textBox.Size != anchorSize)
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

        private void OwnerForm_Deactivate(object? sender, EventArgs e)
        {
            Close();
        }

        private void OwnerForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void DetachActiveState()
        {
            monitorTimer.Stop();

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

        private void DetachTextBox()
        {
            Close();

            if (textBox == null)
                return;

            textBox.Click -= TextBox_Click;
            textBox.TextChanged -= TextBox_TextChanged;
            textBox.Leave -= TextBox_Leave;
            textBox.Disposed -= TextBox_Disposed;

            textBox = null;
        }
    }
}