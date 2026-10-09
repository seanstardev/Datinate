using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    internal sealed class DatGrouperProjectListItem : Control
    {
        private bool isHovering;
        private bool isSelected;
        private string projectName = string.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ProjectName
        {
            get => projectName;
            set
            {
                projectName = value ?? string.Empty;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                Invalidate();
            }
        }

        public DatGrouperProjectListItem()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);

            BackColor = Color.White;
            ForeColor = Color.FromArgb(34, 34, 34);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Height = 30;
            TabStop = false;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovering = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovering = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            Color fillColor = ResolveFillColor();
            Color borderColor = ResolveBorderColor();

            using SolidBrush fillBrush = new SolidBrush(fillColor);
            using Pen borderPen = new Pen(borderColor);
            using Pen highlightPen = new Pen(Color.FromArgb(248, 248, 248));

            e.Graphics.FillRectangle(fillBrush, rect);
            e.Graphics.DrawRectangle(borderPen, rect);
            e.Graphics.DrawLine(highlightPen, 1, 1, Width - 2, 1);

            Rectangle textRect = Rectangle.Inflate(rect, -10, 0);

            TextRenderer.DrawText(
                e.Graphics,
                ProjectName,
                Font,
                textRect,
                ForeColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine);
        }

        private Color ResolveFillColor()
        {
            if (isSelected)
                return Color.FromArgb(220, 234, 244);

            if (isHovering)
                return Color.FromArgb(244, 249, 252);

            return Color.White;
        }

        private Color ResolveBorderColor()
        {
            if (isSelected)
                return Color.FromArgb(126, 170, 198);

            if (isHovering)
                return Color.FromArgb(198, 212, 221);

            return Color.FromArgb(220, 223, 226);
        }
    }
}
