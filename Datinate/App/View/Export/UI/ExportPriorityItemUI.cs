using com.RADIO.Datinate.RMVC.Shared;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    public partial class ExportPriorityItemUI : UserControl
    {
        private const int RowHeightPx = 50;
        private const int IndexWidthPx = 24;
        private const int LeftInsetPx = 30;
        private const int ButtonSizePx = 19;
        private const int ButtonGapPx = 2;
        private const int RightInsetPx = 5;
        private const int CountWidthPx = 100;
        private const int ChipWidthPx = 130;

        private readonly System.Windows.Forms.Timer transferTimer = new();
        private int transferAnimationTicks;

        private readonly Panel actionPanel = new();

        public event Action<ExportPriorityItemUI>? MoveUpRequested;
        public event Action<ExportPriorityItemUI>? MoveDownRequested;
        public event Action<ExportPriorityItemUI>? ExcludeRequested;
        public event Action<ExportPriorityItemUI>? IncludeRequested;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MediaExportPriorityItemDTO? DTO { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExcluded { get; private set; }

        public ExportPriorityItemUI()
        {
            InitializeComponent();

            Height = RowHeightPx;
            MinimumSize = new Size(170, RowHeightPx);
            MaximumSize = new Size(10000, RowHeightPx);
            Margin = new Padding(0, 0, 0, 3);
            BackColor = Color.White;

            button1.Text = string.Empty;
            button2.Text = string.Empty;
            closeRightBtn.Text = string.Empty;

            button1.TabStop = false;
            button2.TabStop = false;
            closeRightBtn.TabStop = false;

            button1.Cursor = Cursors.Hand;
            button2.Cursor = Cursors.Hand;
            closeRightBtn.Cursor = Cursors.Hand;

            ConfigureButton(button1);
            ConfigureButton(button2);
            ConfigureButton(closeRightBtn);

            flowLayoutPanel1.Visible = false;

            actionPanel.BackColor = Color.Transparent;
            actionPanel.Margin = Padding.Empty;
            actionPanel.Padding = Padding.Empty;

            Controls.Add(actionPanel);
            actionPanel.Controls.Add(panel2);
            actionPanel.Controls.Add(closeRightBtn);
            actionPanel.BringToFront();

            panel2.Margin = Padding.Empty;
            panel2.Padding = Padding.Empty;
            panel2.BackColor = Color.Transparent;

            button1.Margin = Padding.Empty;
            button2.Margin = Padding.Empty;
            closeRightBtn.Margin = Padding.Empty;

            panel1.BackColor = Color.FromArgb(224, 234, 244);
            indexLabel.ForeColor = Color.FromArgb(35, 50, 65);
            indexLabel.Font = new Font(indexLabel.Font, FontStyle.Regular);

            mediaLabel.AutoEllipsis = true;
            entryCountLabel.AutoEllipsis = true;

            button1.Click += (_, _) => MoveUpRequested?.Invoke(this);
            button2.Click += (_, _) => MoveDownRequested?.Invoke(this);
            closeRightBtn.Click += (_, _) =>
            {
                if (IsExcluded)
                    IncludeRequested?.Invoke(this);
                else
                    ExcludeRequested?.Invoke(this);
            };

            button1.Paint += (_, e) => DrawArrowButton(e.Graphics, button1.ClientRectangle, true, button1.Enabled);
            button2.Paint += (_, e) => DrawArrowButton(e.Graphics, button2.ClientRectangle, false, button2.Enabled);
            closeRightBtn.Paint += (_, e) => DrawActionButton(e.Graphics, closeRightBtn.ClientRectangle, IsExcluded, closeRightBtn.Enabled);

            transferTimer.Interval = 25;
            transferTimer.Tick += (_, _) =>
            {
                transferAnimationTicks--;

                if (transferAnimationTicks <= 0)
                {
                    transferAnimationTicks = 0;
                    transferTimer.Stop();
                }

                Invalidate();
            };

            Disposed += (_, _) => transferTimer.Dispose();

            Resize += (_, _) => LayoutControls();
            LayoutControls();
        }
        public void PlayTransferAnimation()
        {
            transferAnimationTicks = 10;
            transferTimer.Stop();
            transferTimer.Start();
            Invalidate();
        }
        public void SetUI(MediaExportPriorityItemDTO dto, int index, bool isExcluded = false)
        {
            DTO = dto;

            datChipUI.DatKey = dto.DatGroupEnum.ToString();

            if (!string.IsNullOrWhiteSpace(dto.ResourceSourceName))
                mediaLabel.Text = dto.ResourceSourceName;
            else
            {
                mediaLabel.Text = string.Empty;
            }

            entryCountLabel.Text = "Entries: " + DatinateHelper.GetReadableNumber(dto.EntryCount);

            SetExcluded(isExcluded);
            SetIndex(index);
            LayoutControls();
            BackColor = isExcluded
                ? Color.FromArgb(250, 250, 250)
                : Color.FromArgb(253, 253, 253);
        }

        public void SetIndex(int index)
        {
            indexLabel.Text = IsExcluded ? string.Empty : index.ToString();
        }

        public void SetMoveButtonsEnabled(bool canMoveUp, bool canMoveDown)
        {
            button1.Enabled = canMoveUp && !IsExcluded;
            button2.Enabled = canMoveDown && !IsExcluded;
            button1.Invalidate();
            button2.Invalidate();
        }

        public void SetExcluded(bool isExcluded)
        {
            IsExcluded = isExcluded;

            BackColor = isExcluded
                ? Color.FromArgb(250, 250, 250)
                : Color.White;

            panel1.BackColor = isExcluded
                ? Color.FromArgb(235, 235, 235)
                : Color.FromArgb(224, 234, 244);

            closeRightBtn.Invalidate();
            LayoutControls();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (transferAnimationTicks > 0)
            {
                int alpha = Math.Min(90, transferAnimationTicks * 9);

                using var flashBrush = new SolidBrush(Color.FromArgb(alpha, 180, 210, 240));
                e.Graphics.FillRectangle(flashBrush, ClientRectangle);
            }

            using var bottomPen = new Pen(Color.FromArgb(218, 218, 218));
            e.Graphics.DrawLine(bottomPen, 0, Height - 1, Width, Height - 1);
        }
        private void LayoutControls()
        {
            SuspendLayout();

            int actionButtonHeight = (ButtonSizePx * 2) + ButtonGapPx;

            panel1.SetBounds(0, 0, IndexWidthPx, Height);

            int rightWidth = IsExcluded
                ? ButtonSizePx + RightInsetPx
                : (ButtonSizePx * 2) + ButtonGapPx + RightInsetPx;

            int rightStart = Math.Max(LeftInsetPx + 120, Width - rightWidth);
            int countRight = rightStart - 8;
            int stackTop = (Height - actionButtonHeight) / 2;

            actionPanel.SetBounds(rightStart, 0, rightWidth, Height);

            panel2.Visible = !IsExcluded;
            panel2.SetBounds(0, stackTop, ButtonSizePx, actionButtonHeight);

            button1.SetBounds(
                0,
                0,
                ButtonSizePx,
                ButtonSizePx);

            button2.SetBounds(
                0,
                ButtonSizePx + ButtonGapPx,
                ButtonSizePx,
                ButtonSizePx);

            int actionLeft = IsExcluded
                ? rightWidth - RightInsetPx - ButtonSizePx
                : ButtonSizePx + ButtonGapPx;

            closeRightBtn.SetBounds(
                actionLeft,
                stackTop,
                ButtonSizePx,
                actionButtonHeight);

            int textWidth = Math.Max(20, countRight - LeftInsetPx - CountWidthPx - 8);

            datChipUI.SetBounds(
                LeftInsetPx,
                4,
                Math.Min(ChipWidthPx, Math.Max(80, textWidth)),
                22);

            mediaLabel.SetBounds(
                LeftInsetPx + 1,
                27,
                textWidth,
                18);

            entryCountLabel.SetBounds(
                Math.Max(LeftInsetPx + 40, countRight - CountWidthPx),
                27,
                CountWidthPx,
                18);

            Invalidate();

            ResumeLayout(false);
        }

        private static void ConfigureButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.White;
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderColor = Color.FromArgb(170, 178, 186);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(238, 244, 250);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 232, 244);
            button.Padding = Padding.Empty;
            button.Margin = Padding.Empty;
        }

        private static void DrawArrowButton(Graphics g, Rectangle bounds, bool up, bool enabled)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var brush = new SolidBrush(enabled ? Color.FromArgb(26, 45, 65) : Color.FromArgb(165, 165, 165));

            int cx = bounds.Left + bounds.Width / 2;
            int cy = bounds.Top + bounds.Height / 2;

            Point[] points = up
                ? [
                    new Point(cx, cy - 5),
                    new Point(cx - 6, cy + 4),
                    new Point(cx + 6, cy + 4)
                ]
                : [
                    new Point(cx, cy + 5),
                    new Point(cx - 6, cy - 4),
                    new Point(cx + 6, cy - 4)
                ];

            g.FillPolygon(brush, points);
        }

        private static void DrawActionButton(Graphics g, Rectangle bounds, bool includeMode, bool enabled)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(enabled ? Color.FromArgb(30, 48, 65) : Color.FromArgb(170, 170, 170), 1.6f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            float cx = bounds.Left + bounds.Width / 2f;
            float cy = bounds.Top + bounds.Height / 2f;

            if (includeMode)
            {
                const float arm = 4.5f;

                g.DrawLine(pen, cx - arm, cy, cx + arm, cy);
                g.DrawLine(pen, cx, cy - arm, cx, cy + arm);
                return;
            }

            const float xArm = 5f;

            g.DrawLine(pen, cx - xArm, cy - xArm, cx + xArm, cy + xArm);
            g.DrawLine(pen, cx + xArm, cy - xArm, cx - xArm, cy + xArm);
        }
    }
}