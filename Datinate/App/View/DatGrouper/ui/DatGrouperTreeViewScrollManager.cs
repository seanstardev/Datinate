using System.ComponentModel;

namespace datinate.app
{
    internal sealed class DatGrouperTreeViewScrollManager : IDisposable
    {
        private readonly DatGrouperTreeView treeView;

        private System.Windows.Forms.Timer? pollTimer;

        private const int PollIntervalMs = 75;

        // Couple of extra pixels ensures the complete native scrollbar,
        // including any border/shadow, is outside the parent.
        private int HiddenExtraWidth =>
            SystemInformation.VerticalScrollBarWidth + 2;

        private Size lastParentClientSize = Size.Empty;
        private bool applyingScrollbarPresentation;
        private bool dragOverTree;

        public DatGrouperTreeViewScrollManager(DatGrouperTreeView treeView)
        {
            this.treeView = treeView;

            treeView.DragEnter += TreeView_DragEnter;
            treeView.DragOver += TreeView_DragOver;
            treeView.DragLeave += TreeView_DragLeave;
            treeView.DragDrop += TreeView_DragDrop;
        }


        public void HandleHostHandleCreated()
        {
            lastParentClientSize =
                treeView.Parent?.ClientSize ??
                Size.Empty;

            StartPolling();
            UpdateScrollbarPresentation();
        }

        public void HandleHostHandleDestroyed()
        {
            StopPolling();

            lastParentClientSize =
                Size.Empty;
        }

        public void HandleHostStateChanged()
        {
            if (ShouldPoll())
            {
                StartPolling();
                UpdateScrollbarPresentation();
            }
            else
            {
                StopPolling();
            }
        }

        public void HandleHostMouseEnter()
        {
            UpdateScrollbarPresentation();
        }

        public void HandleHostMouseMove()
        {
            UpdateScrollbarPresentation();
        }

        public void HandleHostMouseLeave()
        {
            UpdateScrollbarPresentation();
        }

        public void HandleHostSizeChanged()
        {
            UpdateScrollbarPresentation();
        }
        public void Dispose()
        {
            treeView.DragEnter -= TreeView_DragEnter;
            treeView.DragOver -= TreeView_DragOver;
            treeView.DragLeave -= TreeView_DragLeave;
            treeView.DragDrop -= TreeView_DragDrop;

            DisposeTimer();
        }
        private void StartPolling()
        {
            if (!ShouldPoll())
                return;

            if (pollTimer == null)
            {
                pollTimer =
                    new System.Windows.Forms.Timer
                    {
                        Interval = PollIntervalMs
                    };

                pollTimer.Tick += PollTimer_Tick;
            }

            if (!pollTimer.Enabled)
                pollTimer.Start();
        }

        private void StopPolling()
        {
            if (pollTimer?.Enabled == true)
                pollTimer.Stop();
        }

        private void DisposeTimer()
        {
            if (pollTimer == null)
                return;

            pollTimer.Stop();
            pollTimer.Tick -= PollTimer_Tick;
            pollTimer.Dispose();

            pollTimer = null;
        }

        private void PollTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (!ShouldPoll())
            {
                StopPolling();
                return;
            }

            UpdateScrollbarPresentation();
        }


        private bool ShouldPoll()
        {
            if (IsDesignTime())
                return false;

            if (treeView.IsDisposed)
                return false;

            if (!treeView.IsHandleCreated)
                return false;

            if (treeView.Parent == null)
                return false;

            if (!treeView.Visible)
                return false;

            if (!treeView.Enabled)
                return false;

            return true;
        }

        private bool IsDesignTime()
        {
            return
                LicenseManager.UsageMode ==
                    LicenseUsageMode.Designtime ||
                (treeView.Site?.DesignMode ?? false);
        }

        private void UpdateScrollbarPresentation()
        {
            if (!ShouldPoll())
                return;

            /*
             * Changing treeView.Width below raises SizeChanged synchronously,
             * which comes back into this manager.
             *
             * Ignore that nested notification. The outer call owns the complete
             * presentation change and will repaint once the final geometry exists.
             */
            if (applyingScrollbarPresentation)
                return;


            var parent =
                treeView.Parent;

            if (parent == null)
                return;


            /*
             * Detect a genuine viewport resize, e.g. SplitContainer movement or
             * application resizing.
             *
             * This is separate from our small artificial TreeView width adjustment
             * used to expose/clip the native scrollbar.
             */
            Size parentClientSize =
                parent.ClientSize;

            bool parentSizeChanged =
                lastParentClientSize != Size.Empty &&
                parentClientSize != lastParentClientSize;

            lastParentClientSize =
                parentClientSize;


            // While the native scrollbar thumb is being dragged, keep the
            // scrollbar-visible geometry even if the cursor leaves the TreeView.
            bool mouseCaptured =
                treeView.Capture &&
                Control.MouseButtons != MouseButtons.None;

            bool mouseOverTreeArea =
                !dragOverTree &&
                (mouseCaptured || IsMouseOverVisibleTreeArea(parent));

            int desiredRight =
                parent.ClientSize.Width +
                (mouseOverTreeArea
                    ? 0
                    : HiddenExtraWidth);

            int desiredWidth =
                desiredRight -
                treeView.Left;

            if (desiredWidth < 1)
                return;


            bool treeWidthChanged =
                treeView.Width != desiredWidth;


            /*
             * Apply our scrollbar presentation geometry first.
             *
             * SizeChanged is synchronous, so when this block completes all of the
             * manager-owned geometry work has happened.
             */
            if (treeWidthChanged)
            {
                applyingScrollbarPresentation = true;

                try
                {
                    treeView.Width =
                        desiredWidth;
                }
                finally
                {
                    applyingScrollbarPresentation = false;
                }
            }


            /*
             * REAL VIEWPORT RESIZE
             *
             * Right-aligned/bespoke node rendering can move over a large area when
             * the SplitContainer changes width, so repaint the complete TreeView.
             *
             * Do not synchronously Update() here: splitter dragging can generate
             * many resize events and Windows can efficiently coalesce them.
             */
            if (parentSizeChanged)
            {
                treeView.Invalidate();
                return;
            }


            /*
             * No geometry changed.
             *
             * This is the normal timer-poll path and should be essentially free.
             */
            if (!treeWidthChanged)
                return;


            /*
             * SCROLLBAR PRESENTATION CHANGE
             *
             * This is the important part.
             *
             * The TreeView has now reached its FINAL width. Force one complete
             * repaint so all owner-drawn nodes render from that final geometry.
             *
             * This is deliberately synchronous. It is not a timing workaround:
             *
             *      change geometry
             *          ↓
             *      geometry completes
             *          ↓
             *      invalidate
             *          ↓
             *      repaint now
             *
             * It happens only when scrollbar presentation actually changes,
             * not on every poll tick.
             */
            treeView.Invalidate();
            treeView.Update();


            /*
             * The native scrollbar itself sits at/over the parent's clipping
             * boundary. Repaint that small parent edge as well to remove any
             * non-client residue left by the width transition.
             */
            int parentStripWidth =
                HiddenExtraWidth + 2;

            int parentStripLeft =
                Math.Max(
                    0,
                    parent.ClientSize.Width -
                    parentStripWidth);

            var parentRepaintRect =
                new Rectangle(
                    parentStripLeft,
                    0,
                    parent.ClientSize.Width -
                    parentStripLeft,
                    parent.ClientSize.Height);

            parent.Invalidate(
                parentRepaintRect,
                false);

            parent.Update();
        }

        private bool IsMouseOverVisibleTreeArea(
            Control parent)
        {
            var point =
                parent.PointToClient(Cursor.Position);

            // treeView.Left is deliberately negative in this UI, so use the
            // parent's visible left/right boundaries rather than the TreeView's
            // complete native window bounds.
            int top =
                treeView.Top;

            int bottom =
                Math.Min(
                    treeView.Bottom,
                    parent.ClientSize.Height);

            return
                point.X >= 0 &&
                point.X < parent.ClientSize.Width &&
                point.Y >= top &&
                point.Y < bottom;
        }

        private void TreeView_DragEnter(object? sender, DragEventArgs e)
        {
            SetDragOverTree(true);
        }

        private void TreeView_DragOver(object? sender, DragEventArgs e)
        {
            SetDragOverTree(true);
        }

        private void TreeView_DragLeave(object? sender, EventArgs e)
        {
            SetDragOverTree(false);
        }

        private void TreeView_DragDrop(object? sender, DragEventArgs e)
        {
            SetDragOverTree(false);
        }

        private void SetDragOverTree(bool value)
        {
            if (dragOverTree == value)
                return;

            dragOverTree = value;
            UpdateScrollbarPresentation();
        }
    }
}