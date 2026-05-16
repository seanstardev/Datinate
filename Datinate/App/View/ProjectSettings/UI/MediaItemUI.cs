using System.ComponentModel;

namespace datinate.app
{
    public partial class MediaItemUI : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? IconImageKey { get => mediaIconUI.ImageKey; set => mediaIconUI.ImageKey = value; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MediaDescription
        {
            get => descriptionLabel.Text;
            set
            {
                descriptionLabel.Text = value;

                if (string.IsNullOrWhiteSpace(value))
                {
                    descriptionLabel.Visible = false;
                    datChipUI.Location = new Point(
                        datChipUI.Location.X,
                        _defaultChipY + 10);
                }
                else
                {
                    descriptionLabel.Visible = true;
                    
                    datChipUI.Location = new Point(
                        datChipUI.Location.X,
                        _defaultChipY);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IconAlpha
        {
            get => mediaIconUI.Alpha;
            set => mediaIconUI.Alpha = value;
            
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DatChipKey
        {
            get => datChipUI.DatKey;
            set
            {
                datChipUI.DatKey = value;
                
                if (string.IsNullOrWhiteSpace(value))
                {
                    datChipUI.Visible = false;
                    descriptionLabel.Location = new Point(
                        descriptionLabel.Location.X,
                        _defaultLabelY - 12);
                }
                else
                {
                    datChipUI.Visible = true;
                    descriptionLabel.Location = new Point(
                        descriptionLabel.Location.X,
                        _defaultLabelY);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HideIcon 
        { 
            get => _hideIcon; 
            set
            {
                if (_hideIcon == value) return;
                else if (value)
                {
                    datChipUI.Location = new Point(
                        datChipUI.Location.X - mediaIconUI.Size.Width,
                        datChipUI.Location.Y);

                    descriptionLabel.Location = new Point(
                        descriptionLabel.Location.X - mediaIconUI.Size.Width,
                        descriptionLabel.Location.Y);

                    mediaIconUI.Visible = false;
                }
                else
                {
                    datChipUI.Location = new Point(
                        datChipUI.Location.X + mediaIconUI.Size.Width,
                        datChipUI.Location.Y);

                    descriptionLabel.Location = new Point(
                        descriptionLabel.Location.X + mediaIconUI.Size.Width,
                        descriptionLabel.Location.Y);

                    mediaIconUI.Visible = true;
                }

                _hideIcon = value;
            }
        }
        private bool _hideIcon = false;
        private readonly int _defaultLabelY;
        private readonly int _defaultChipY;

        public MediaItemUI()
        {
            InitializeComponent();

            Size = new Size(Size.Width, 48);
            _defaultLabelY = descriptionLabel.Location.Y;
            _defaultChipY = datChipUI.Location.Y;
            MaximumSize = new Size(MaximumSize.Width, 48);
            MinimumSize = new Size(MinimumSize.Width, 48);
        }
    }
}
