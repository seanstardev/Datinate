namespace datinate.app
{
    internal sealed class ProjectsListBoxUI : ListBox
    {
        private int hoveredProjectIndex = -1;

        public ProjectsListBoxUI()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 34;
            IntegralHeight = false;
            BorderStyle = BorderStyle.None;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(25, 25, 25);
            Cursor = Cursors.Default;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0 || e.Index >= Items.Count)
                return;

            var bounds = e.Bounds;
            bool selected =
                (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            bool hovered =
                e.Index == hoveredProjectIndex;

            Color backColor = Color.White;
            Color textColor = Color.FromArgb(24, 24, 24);
            Color accentColor = Color.FromArgb(35, 114, 198);

            if (selected)
            {
                backColor = Color.FromArgb(226, 238, 252);
                textColor = Color.FromArgb(16, 52, 92);
            }
            else if (hovered)
            {
                backColor = Color.FromArgb(242, 247, 252);
            }
            else if ((e.Index & 1) == 1)
            {
                backColor = Color.FromArgb(249, 250, 252);
            }

            using (var backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, bounds);
            }

            using (var font = new Font(
                Font,
                selected ? FontStyle.Bold : FontStyle.Regular))
            {
                var textRect = Rectangle.Inflate(bounds, -12, 0);

                TextRenderer.DrawText(
                    e.Graphics,
                    Items[e.Index]?.ToString() ?? string.Empty,
                    font,
                    textRect,
                    textColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis);
            }

            if (selected)
            {
                using var accentBrush = new SolidBrush(accentColor);

                e.Graphics.FillRectangle(
                    accentBrush,
                    bounds.Left,
                    bounds.Top,
                    4,
                    bounds.Height);
            }
            else
            {
                using var dividerPen =
                    new Pen(Color.FromArgb(232, 236, 241));

                e.Graphics.DrawLine(
                    dividerPen,
                    bounds.Left + 8,
                    bounds.Bottom - 1,
                    bounds.Right - 8,
                    bounds.Bottom - 1);
            }

            e.DrawFocusRectangle();

            base.OnDrawItem(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int index = IndexFromPoint(e.Location);

            Cursor = index >= 0
                ? Cursors.Hand
                : Cursors.Default;

            if (hoveredProjectIndex == index)
                return;

            int oldIndex = hoveredProjectIndex;
            hoveredProjectIndex = index;

            InvalidateItem(oldIndex);
            InvalidateItem(hoveredProjectIndex);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            Cursor = Cursors.Default;

            if (hoveredProjectIndex < 0)
                return;

            int oldIndex = hoveredProjectIndex;
            hoveredProjectIndex = -1;

            InvalidateItem(oldIndex);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            EnsureSelectedProjectVisibleDeferred();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);

            EnsureSelectedProjectVisibleDeferred();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
                EnsureSelectedProjectVisibleDeferred();
        }

        public void EnsureSelectedProjectVisibleDeferred()
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
                return;

            BeginInvoke((Action)(() =>
            {
                if (IsDisposed || Disposing || !IsHandleCreated)
                    return;

                EnsureSelectedProjectVisible();
            }));
        }

        private void EnsureSelectedProjectVisible()
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
                return;

            if (!Visible || Items.Count == 0)
                return;

            int selectedIndex = SelectedIndex;

            if (selectedIndex < 0 || selectedIndex >= Items.Count)
                return;

            int itemHeight = Math.Max(1, ItemHeight);

            int visibleCount =
                Math.Max(1, ClientSize.Height / itemHeight);

            int topIndex = TopIndex;
            int bottomIndex = topIndex + visibleCount - 1;

            int targetTopIndex = topIndex;

            if (selectedIndex < topIndex)
            {
                targetTopIndex = selectedIndex;
            }
            else if (selectedIndex > bottomIndex)
            {
                targetTopIndex =
                    selectedIndex - visibleCount + 1;
            }
            else
            {
                return;
            }

            int maxTopIndex =
                Math.Max(0, Items.Count - visibleCount);

            targetTopIndex =
                Math.Max(
                    0,
                    Math.Min(targetTopIndex, maxTopIndex));

            if (TopIndex == targetTopIndex)
                return;

            TopIndex = targetTopIndex;
            Invalidate();
        }

        private void InvalidateItem(int index)
        {
            if (index < 0 || index >= Items.Count)
                return;

            Invalidate(GetItemRectangle(index));
        }
    }
}