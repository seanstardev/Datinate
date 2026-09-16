using com.RADIO.Datinate.RMVC.Shared;
using System.Drawing.Drawing2D;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class ScoringRowUI : UserControl
    {
        public ScoringRowUI()
        {
            InitializeComponent();

            UpdateRoundedRegion();
        }

        public void SetUI(DatGrouperScoringItem item)
        {
            Ui(() =>
            {
                try
                {
                    SuspendLayout();

                    if (item.Icon2 == null)
                    {
                        mediaIcon2.Image = item.Icon1;
                        mediaIcon1.Visible = false;
                    }
                    else
                    {
                        mediaIcon1.Visible = true;
                        mediaIcon1.Image = item.Icon1;
                        mediaIcon2.Image = item.Icon2;
                    }

                    mediaLabel.Text = item.MediaTypeEnum.ToString().Replace("_", ": ");

                    datChipContainer.Controls.Clear();

                    int sourceCount = 0;

                    HashSet<string> addedSourceIDs = [];


                    foreach (var sourceId in item.SourceIds)
                    {
                        string chipLabel = string.Empty;

                        sourceCount++;

                        if (addedSourceIDs.Contains(sourceId))
                            continue;
                        else
                            _ = addedSourceIDs.Add(sourceId);

                        var group = DatinatePointerHelper.GetDatGroup(sourceId);
                        
                        if (group == DAT_GROUP_ENUM.R2DAT_WEB)
                        {
                            // TODO: Should be in RadioDatHelper. Must be more robust.
                            chipLabel = GetLastColonSegment(sourceId); 
                        }
                        var chip = new DatChipUI()
                        {
                            DatKey = sourceId,
                            Margin = new Padding(0, 0, 4, 4),
                            ChipLabel = chipLabel
                        };

                        datChipContainer.Controls.Add(chip);
                    }

                    confirmationPictureBox.AcceptedCount = item.HasScore
                        ? Math.Max(1, sourceCount)
                        : 0;
                }
                finally
                {
                    ResumeLayout();
                }
            });
        }
        private string GetLastColonSegment(string s)
        {
            int i = s.LastIndexOf(':');
            return i < 0 ? s.Trim() : s[(i + 1)..].Trim();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            UpdateRoundedRegion();
        }

        private void UpdateRoundedRegion()
        {
            if (Width <= 0 || Height <= 0)
                return;

            const int radius = 6;
            int diameter = radius * 2;

            var rect = new Rectangle(0, 0, Width, Height);

            using var path = new GraphicsPath();

            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            var oldRegion = Region;
            Region = new Region(path);
            oldRegion?.Dispose();
        }
        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
                return;
            }

            action();
        }
    }
}