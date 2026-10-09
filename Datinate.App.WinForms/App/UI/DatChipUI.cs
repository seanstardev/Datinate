using System.ComponentModel;

namespace datinate.app
{
    public sealed class DatChipUI : Control
    {
        private string datKey = string.Empty;
        private string chipLabel = string.Empty;

        private bool small;
        private bool strong;

        private const int LabelGapPx = 2;
        private const int LabelOverhangPadPx = 6;

        [DefaultValue("")]

        // optional but nice: shows in Properties grid under Appearance
        [Category("Appearance")]
        public string DatKey
        {
            get => datKey;
            set
            {
                value ??= string.Empty;
                if (string.Equals(datKey, value, StringComparison.Ordinal))
                    return;

                datKey = value;
                RefreshChipSize();
                Invalidate();
            }
        }

        [DefaultValue("")]
        [Category("Appearance")]
        public string ChipLabel
        {
            get => chipLabel;
            set
            {
                value ??= string.Empty;
                if (string.Equals(chipLabel, value, StringComparison.Ordinal))
                    return;

                chipLabel = value;
                RefreshChipSize();
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

        [DefaultValue(false)]
        [Category("Appearance")]
        public bool Strong
        {
            get => strong;
            set
            {
                if (strong == value)
                    return;

                strong = value;
                RefreshChipSize();
                Invalidate();
            }
        }

        public DatChipUI()
        {
            Margin = new Padding(0);
            Padding = new Padding(0);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            TabStop = false;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            RefreshChipSize();
            Invalidate();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var s = MeasureChipSize();
            return new Size(Math.Max(1, s.Width), Math.Max(1, s.Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (string.IsNullOrWhiteSpace(datKey))
                return;

            // If the chip background has rounded/transparent edges, clearing helps avoid fringe artifacts.
            var back = Parent?.BackColor ?? BackColor;
            using (var bg = new SolidBrush(back))
                e.Graphics.FillRectangle(bg, ClientRectangle);

            var chipSize = DatChipUtil.Measure(datKey, small, strong);

            if (small)
                DatChipUtil.DrawSmall(e.Graphics, datKey, 0, 0, strongBorder: strong);
            else
                DatChipUtil.Draw(e.Graphics, datKey, 0, 0, strongBorder: strong);

            if (!string.IsNullOrWhiteSpace(chipLabel))
            {
                int x = chipSize.Width + LabelGapPx;

                var flags = TextFormatFlags.NoPadding |
                            TextFormatFlags.SingleLine |
                            TextFormatFlags.VerticalCenter |
                            TextFormatFlags.Left;

                var labelRect = new Rectangle(
                    x,
                    1,
                    Math.Max(1, Width - x),
                    Height);

                TextRenderer.DrawText(e.Graphics, chipLabel, Font, labelRect, ForeColor, flags);
            }
        }

        private void RefreshChipSize()
        {
            var s = MeasureChipSize();

            // Lock it so layout engines don’t “helpfully” resize it.
            Size = s;
            MinimumSize = s;
            MaximumSize = s;
        }

        private Size MeasureChipSize()
        {
            var chipSize = DatChipUtil.Measure(datKey, small, strong);

            if (string.IsNullOrWhiteSpace(chipLabel))
                return chipSize;

            var flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
            var labelSize = TextRenderer.MeasureText(chipLabel, Font, new Size(int.MaxValue, int.MaxValue), flags);

            int w = chipSize.Width + LabelGapPx + labelSize.Width + LabelOverhangPadPx;
            return new Size(w, chipSize.Height);
        }
    }
}