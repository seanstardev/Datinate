using System.ComponentModel;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public sealed class DatGroupChipButton : Control
    {
        private DAT_GROUP_ENUM datGroupEnum = DAT_GROUP_ENUM.NOT_SET;
        private string datKey = string.Empty;
        private bool selected;
        private bool muted;
        private bool small;

        private const int OuterPaddingPx = 3;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DAT_GROUP_ENUM DatGroupEnum
        {
            get => datGroupEnum;
            set
            {
                if (datGroupEnum == value)
                    return;

                datGroupEnum = value;
                datKey = value == DAT_GROUP_ENUM.NOT_SET ? string.Empty : value.ToString();
                RefreshChipSize();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selected
        {
            get => selected;
            set
            {
                if (selected == value)
                    return;

                selected = value;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Muted
        {
            get => muted;
            set
            {
                if (muted == value)
                    return;

                muted = value;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        [Category("Appearance")]
        public bool Small
        {
            get => small;
            set
            {
                if (small == value)
                    return;

                small = value;
                RefreshChipSize();
                Invalidate();
            }
        }

        public DatGroupChipButton()
        {
            Cursor = Cursors.Hand;
            Margin = new Padding(0);
            Padding = new Padding(0);
            TabStop = false;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            RefreshChipSize();
            Invalidate();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size chipSize = MeasureChipSize();

            return new Size(
                chipSize.Width + (OuterPaddingPx * 2),
                chipSize.Height + (OuterPaddingPx * 2));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color backColor = Parent?.BackColor ?? BackColor;

            using (SolidBrush brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, ClientRectangle);

            if (string.IsNullOrWhiteSpace(datKey))
                return;

            Size chipSize = DatChipUtil.Measure(datKey, small, selected);

            int x = OuterPaddingPx;
            int y = OuterPaddingPx;

            if (small)
                DatChipUtil.DrawSmall(e.Graphics, datKey, x, y, strongBorder: selected);
            else
                DatChipUtil.Draw(e.Graphics, datKey, x, y, strongBorder: selected);

            Rectangle chipRect = new Rectangle(x, y, chipSize.Width, chipSize.Height);

            if (muted && !selected)
            {
                using SolidBrush mutedBrush = new SolidBrush(Color.FromArgb(150, backColor));
                e.Graphics.FillRectangle(mutedBrush, chipRect);
            }

            if (selected)
            {
                Rectangle borderRect = new Rectangle(0, 0, Width - 1, Height - 1);

                using Pen pen = new Pen(SystemColors.Highlight, 2);
                e.Graphics.DrawRectangle(pen, borderRect);
            }
        }

        private void RefreshChipSize()
        {
            Size preferredSize = GetPreferredSize(Size.Empty);

            Size = preferredSize;
            MinimumSize = preferredSize;
            MaximumSize = preferredSize;
        }

        private Size MeasureChipSize()
        {
            if (string.IsNullOrWhiteSpace(datKey))
                return new Size(1, 1);

            return DatChipUtil.Measure(datKey, small, selected);
        }
    }
}