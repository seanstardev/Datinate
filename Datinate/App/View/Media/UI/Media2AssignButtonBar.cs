using System.ComponentModel;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public sealed class Media2AssignButtonBar : UserControl
    {
        public MEDIA_ASSIGNMENT_ENUM AssignmentStatus => selected;

        public event Action<MEDIA_ASSIGNMENT_ENUM>? SelectedChanged;

        private readonly Button btnAssigned;
        private readonly Button btnUnassigned;
        private readonly Button btnNotFound;

        private MEDIA_ASSIGNMENT_ENUM selected = MEDIA_ASSIGNMENT_ENUM.Assigned;

        private Font? regularFont;
        private Font? boldFont;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public MEDIA_ASSIGNMENT_ENUM Selected
        {
            get => selected;
            set => SetSelectedInternal(value, raiseEvent: false); // programmatic set = no event
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color AssignedSelectedBackColor { get; set; } = Color.FromArgb(224, 245, 224);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color UnassignedSelectedBackColor { get; set; } = Color.FromArgb(235, 235, 235);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color NotFoundSelectedBackColor { get; set; } = Color.FromArgb(235, 235, 235);

        public Media2AssignButtonBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Height = 24;

            btnAssigned = CreateButton("Assign");
            btnUnassigned = CreateButton("← Search");
            btnNotFound = CreateButton("Not Found");

            btnAssigned.Cursor = Cursors.Hand;
            btnNotFound.Cursor = Cursors.Hand;
            btnUnassigned.Cursor = Cursors.Hand;

            btnAssigned.Click += (_, __) => SetSelectedInternal(MEDIA_ASSIGNMENT_ENUM.Assigned, raiseEvent: true);
            btnUnassigned.Click += (_, __) => SetSelectedInternal(MEDIA_ASSIGNMENT_ENUM.None, raiseEvent: true);
            btnNotFound.Click += (_, __) => SetSelectedInternal(MEDIA_ASSIGNMENT_ENUM.NotFound, raiseEvent: true);


            Controls.Add(btnAssigned);
            Controls.Add(btnUnassigned);
            Controls.Add(btnNotFound);

            SizeChanged += (_, __) => LayoutButtons();
            FontChanged += (_, __) =>
            {
                UpdateFonts();
                ApplyVisualState();
            };

            UpdateFonts();
            LayoutButtons();
            ApplyVisualState();
        }
        private void SetSelectedInternal(MEDIA_ASSIGNMENT_ENUM value, bool raiseEvent)
        {
            selected = value;
            ApplyVisualState();

            if (raiseEvent)
                SelectedChanged?.Invoke(selected);
        }
        private Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                TextAlign = ContentAlignment.MiddleCenter,
                UseMnemonic = false,
                AutoSize = false
            };
        }

        private void UpdateFonts()
        {
            regularFont?.Dispose();
            boldFont?.Dispose();

            regularFont = new Font(Font, FontStyle.Regular);
            boldFont = new Font(Font, FontStyle.Bold);
        }

        private void LayoutButtons()
        {
            var w = ClientSize.Width;
            var h = ClientSize.Height;

            if (w <= 0 || h <= 0)
                return;

            int w1 = w / 3;
            int w2 = w / 3;
            int w3 = w - w1 - w2;

            btnAssigned.SetBounds(0, 0, w1, h);
            btnUnassigned.SetBounds(w1, 0, w2, h);
            btnNotFound.SetBounds(w1 + w2, 0, w3, h);

            btnAssigned.FlatAppearance.BorderSize = 1;
            btnUnassigned.FlatAppearance.BorderSize = 1;
            btnNotFound.FlatAppearance.BorderSize = 1;

            btnAssigned.FlatAppearance.BorderColor = SystemColors.ControlDark;
            btnUnassigned.FlatAppearance.BorderColor = SystemColors.ControlDark;
            btnNotFound.FlatAppearance.BorderColor = SystemColors.ControlDark;

            btnAssigned.FlatAppearance.MouseOverBackColor = SystemColors.ControlLight;
            btnUnassigned.FlatAppearance.MouseOverBackColor = SystemColors.ControlLight;
            btnNotFound.FlatAppearance.MouseOverBackColor = SystemColors.ControlLight;

            btnAssigned.FlatAppearance.MouseDownBackColor = SystemColors.ControlDark;
            btnUnassigned.FlatAppearance.MouseDownBackColor = SystemColors.ControlDark;
            btnNotFound.FlatAppearance.MouseDownBackColor = SystemColors.ControlDark;

            ApplyBorderJoining();
        }

        private void ApplyBorderJoining()
        {
            btnUnassigned.Left -= 1;
            btnUnassigned.Width += 2;

            btnNotFound.Left -= 1;
            btnNotFound.Width += 1;
        }

        private void ApplyVisualState()
        {
            ApplyButtonState(btnAssigned, MEDIA_ASSIGNMENT_ENUM.Assigned, selected == MEDIA_ASSIGNMENT_ENUM.Assigned);
            ApplyButtonState(btnUnassigned, MEDIA_ASSIGNMENT_ENUM.None, selected == MEDIA_ASSIGNMENT_ENUM.None);
            ApplyButtonState(btnNotFound, MEDIA_ASSIGNMENT_ENUM.NotFound, selected == MEDIA_ASSIGNMENT_ENUM.NotFound);
        }

        private void ApplyButtonState(Button b, MEDIA_ASSIGNMENT_ENUM segment, bool isSelected)
        {
            b.Font = isSelected ? boldFont! : regularFont!;
            b.BackColor = isSelected ? GetSelectedBackColor(segment) : SystemColors.Control;
            b.ForeColor = SystemColors.ControlText;
        }

        private Color GetSelectedBackColor(MEDIA_ASSIGNMENT_ENUM s) => s switch
        {
            MEDIA_ASSIGNMENT_ENUM.Assigned => AssignedSelectedBackColor,
            MEDIA_ASSIGNMENT_ENUM.None => UnassignedSelectedBackColor,
            MEDIA_ASSIGNMENT_ENUM.NotFound => NotFoundSelectedBackColor,
            _ => SystemColors.ControlLightLight
        };

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                regularFont?.Dispose();
                boldFont?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
