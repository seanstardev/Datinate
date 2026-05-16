namespace datinate.app
{
    public static class TreeViewAdornerUtil
    {
        public enum INSERTION_POINT_ENUM
        {
            Before,
            None,
            After
        }

        private static readonly InsertAdorner Adorner = new InsertAdorner();

        public static void Clear() => Adorner.Clear();

        public static void Update(
            DatGrouperTreeView treeView,
            TreeNode? node,
            INSERTION_POINT_ENUM mode)
            => Adorner.Update(treeView, node, mode);

        public static void Draw(DatGrouperTreeView treeView, DrawTreeNodeEventArgs e)
            => Adorner.Draw(treeView, e);

        private sealed class InsertAdorner
        {
            public const int LineThicknessPx = 2;
            public const int LinePadLeftPx = 0;
            public const int LinePadRightPx = 0;

            private TreeView? hostTreeView;
            private TreeNode? node;
            private INSERTION_POINT_ENUM mode;

            public void Clear()
            {
                if (hostTreeView != null && node != null && mode != INSERTION_POINT_ENUM.None)
                    hostTreeView.Invalidate(GetInvalidateRect(hostTreeView, node, mode));

                node = null;
                mode = INSERTION_POINT_ENUM.None;
                hostTreeView = null;
            }

            public void Update(
                TreeView? tv,
                TreeNode? nextNode,
                INSERTION_POINT_ENUM nextMode)
            {
                if (tv == null || nextNode == null || nextMode == INSERTION_POINT_ENUM.None)
                {
                    Clear();
                    return;
                }

                if (!ReferenceEquals(hostTreeView, tv))
                {
                    Clear();
                    hostTreeView = tv;
                }

                Set(tv, nextNode, nextMode);
            }

            public void Draw(TreeView tv, DrawTreeNodeEventArgs e)
            {
                if (mode == INSERTION_POINT_ENUM.None || node == null)
                    return;

                if (!ReferenceEquals(hostTreeView, tv))
                    return;

                if (!ReferenceEquals(e.Node, node))
                    return;

                var b = e.Node.Bounds;
                var y = mode == INSERTION_POINT_ENUM.Before ? b.Top : b.Bottom - 1;

                var c = SystemColors.Highlight;
                var lineColor = Color.FromArgb(170, c.R, c.G, c.B);

                using var pen = new Pen(lineColor, LineThicknessPx);

                var x1 = LinePadLeftPx;
                var x2 = Math.Max(x1 + 1, tv.ClientSize.Width - 1 - LinePadRightPx);

                e.Graphics.DrawLine(pen, x1, y, x2, y);
            }

            private void Set(TreeView tv, TreeNode nextNode, INSERTION_POINT_ENUM nextMode)
            {
                if (ReferenceEquals(node, nextNode) && mode == nextMode)
                    return;

                if (node != null && mode != INSERTION_POINT_ENUM.None)
                    tv.Invalidate(GetInvalidateRect(tv, node, mode));

                node = nextNode;
                mode = nextMode;

                if (node != null && mode != INSERTION_POINT_ENUM.None)
                    tv.Invalidate(GetInvalidateRect(tv, node, mode));
            }

            private static Rectangle GetInvalidateRect(
                TreeView tv,
                TreeNode node,
                INSERTION_POINT_ENUM mode)
            {
                var b = node.Bounds;
                var y = mode == INSERTION_POINT_ENUM.Before ? b.Top : b.Bottom - 1;

                var top = Math.Max(0, y - (LineThicknessPx + 2));
                var height = (LineThicknessPx + 2) * 2 + 1;

                return new Rectangle(
                    0,
                    top,
                    tv.ClientSize.Width,
                    Math.Min(height, tv.ClientSize.Height - top));
            }
        }
    }
}
