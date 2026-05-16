using com.RADIO.Datinate.RMVC.Shared;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class ScoringUI : UserControl
    {
        private const int TopInsetPx = 25;
        private const int BottomInsetPx = 20;
        private const int MinHeightPx = TopInsetPx + BottomInsetPx;

        public ScoringUI()
        {
            InitializeComponent();

            rowContainer.FlowDirection = FlowDirection.TopDown;
            rowContainer.WrapContents = false;
            rowContainer.AutoScroll = true;
        }

        public void ClearUI()
        {
            Ui(() =>
            {
                SuspendLayout();
                rowContainer.SuspendLayout();

                rowContainer.Controls.Clear();

                rowContainer.ResumeLayout(true);
                RefreshViewportLayout();

                ResumeLayout(true);
            });
        }

        public void SetUI(DatGrouperScoring scoring)
        {
            Ui(() =>
            {
                SuspendLayout();
                rowContainer.SuspendLayout();

                try
                {
                    BackColor = scoring.IsScoringExempt ? Color.Orange : SystemColors.Control;

                    topLeftPB.BackColor =
                    topRightPB.BackColor =
                    bottomLeftPB.BackColor =
                    bottomRightPB.BackColor =
                    rowContainer.BackColor =
                        BackColor;

                    rowContainer.Controls.Clear();

                    int totalRows = scoring.ResourceDictionary.Count + scoring.MediaDictionary.Count;
                    int rowIndex = 0;

                    foreach (var resource in scoring.ResourceDictionary)
                    {
                        bool isLast = rowIndex == totalRows - 1;
                        AddRow(resource.Key, resource.Value, isLast);
                        rowIndex++;
                    }

                    foreach (var media in scoring.MediaDictionary)
                    {
                        bool isLast = rowIndex == totalRows - 1;
                        AddRow(media.Key, media.Value, isLast);
                        rowIndex++;
                    }
                }
                finally
                {
                    rowContainer.ResumeLayout(true);
                    RefreshViewportLayout();
                    ResumeLayout(true);
                }
            });
        }

        private void AddRow(MEDIA_TYPE_ENUM key, DatGrouperScoringItem item, bool isLast)
        {
            int bottomMargin = isLast ? 0 : 4;
            var ui = new ScoringRowUI
            {
                Margin = new Padding(4, 0, 4, bottomMargin)
            };

            rowContainer.Controls.Add(ui);
            ui.SetUI(item);
        }

        public int GetPreferredHeightPx()
        {
            int rowsHeight = rowContainer.Padding.Top + rowContainer.Padding.Bottom;

            for (int i = 0; i < rowContainer.Controls.Count; i++)
            {
                var c = rowContainer.Controls[i];
                if (!c.Visible)
                    continue;

                rowsHeight += c.Margin.Top + c.Height + c.Margin.Bottom;
            }

            int total = TopInsetPx + rowsHeight + BottomInsetPx;
            return Math.Max(MinHeightPx, total);
        }

        public void RefreshViewportLayout()
        {
            if (IsDisposed)
                return;

            int bodyHeight = Height - TopInsetPx - BottomInsetPx;
            if (bodyHeight < 0)
                bodyHeight = 0;

            int bodyWidth = ClientSize.Width;
            if (bodyWidth < 0)
                bodyWidth = 0;

            if (rowContainer.Location.X != 0 || rowContainer.Location.Y != TopInsetPx)
                rowContainer.Location = new Point(0, TopInsetPx);

            if (rowContainer.Width != bodyWidth || rowContainer.Height != bodyHeight)
                rowContainer.Size = new Size(bodyWidth, bodyHeight);

            int contentHeight = rowContainer.Padding.Top + rowContainer.Padding.Bottom;

            for (int i = 0; i < rowContainer.Controls.Count; i++)
            {
                var c = rowContainer.Controls[i];
                if (!c.Visible)
                    continue;

                int rowH = c.Margin.Top + c.Height + c.Margin.Bottom;
                contentHeight += rowH;
            }

            if (contentHeight < 0)
                contentHeight = 0;

            var minSize = new Size(0, contentHeight);
            if (rowContainer.AutoScrollMinSize != minSize)
                rowContainer.AutoScrollMinSize = minSize;

            bool needsVScroll = contentHeight > rowContainer.ClientSize.Height;

            int viewportWidth = bodyWidth - rowContainer.Padding.Left - rowContainer.Padding.Right;
            if (needsVScroll)
                viewportWidth -= SystemInformation.VerticalScrollBarWidth;
            viewportWidth -= 1;

            if (viewportWidth < 0)
                viewportWidth = 0;

            rowContainer.SuspendLayout();
            try
            {
                for (int i = 0; i < rowContainer.Controls.Count; i++)
                {
                    var c = rowContainer.Controls[i];
                    if (!c.Visible)
                        continue;

                    int targetWidth = viewportWidth - c.Margin.Left - c.Margin.Right;
                    if (targetWidth < 0)
                        targetWidth = 0;

                    if (c.Width != targetWidth)
                        c.Width = targetWidth;
                }
            }
            finally
            {
                rowContainer.ResumeLayout(true);
            }
        }

        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated)
                    return;

                BeginInvoke(action);
                return;
            }

            action();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RefreshViewportLayout();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
                RefreshViewportLayout();
        }
    }
}