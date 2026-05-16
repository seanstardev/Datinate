namespace datinate.app
{
    public interface IDatGrouperNodePreview
    {
        void ClearPreview();
        void SetPreviewSource(DatGrouperUiBase? owner, TreeNode? node);
        void DrawPreview();
    }

    public class DatGrouperNodePreviewUI : Control, IDatGrouperNodePreview
    {
        private DatGrouperUiBase? owner;
        private TreeNode? node;

        public DatGrouperNodePreviewUI()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            TabStop = false;
            BackColor = SystemColors.Window;
        }

        public void SetPreviewSource(DatGrouperUiBase? owner, TreeNode? node)
        {
            this.owner = owner;
            this.node = node;
            Invalidate();
        }

        public void DrawPreview()
        {
            Invalidate();
        }

        public void ClearPreview()
        {
            owner = null;
            node = null;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(BackColor);

            if (owner == null || node == null)
                return;

            owner.DrawPreviewNode(node, ClientRectangle, e.Graphics);
        }

        protected override void OnMouseDown(MouseEventArgs e) { }
        protected override void OnMouseUp(MouseEventArgs e) { }
        protected override void OnMouseMove(MouseEventArgs e) { }
        protected override void OnMouseWheel(MouseEventArgs e) { }
        protected override void OnDoubleClick(EventArgs e) { }
        protected override bool IsInputKey(Keys keyData) => false;
        protected override void OnGotFocus(EventArgs e) { }
    }
}