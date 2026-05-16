using com.RADIO.Datinate.RMVC.Shared;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class ExportPriorityUI : UserControl
    {
        private const int PreferredWidthPx = 320;

        private const int SectionHeaderHeightPx = 20;

        private MEDIA_TYPE_ENUM mediaType = MEDIA_TYPE_ENUM.NOT_SET;
        private AnimatedExportListPanelUI? includeListPanel;
        private AnimatedExportListPanelUI? excludeListPanel;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MEDIA_TYPE_ENUM MediaType => mediaType;

        public ExportPriorityUI()
        {
            InitializeComponent();

            MinimumSize = new Size(PreferredWidthPx, Height);
            Width = Math.Max(Width, PreferredWidthPx);

            panel1.BackColor = Color.FromArgb(246, 246, 246);
            panel3.BackColor = Color.White;
            panel4.BackColor = Color.White;

            includeContainer.WrapContents = false;
            flowLayoutPanel2.WrapContents = false;

            ConfigureContainerChrome();
        }
        private void ConfigureContainerChrome()
        {
            panel3.BorderStyle = BorderStyle.None;
            panel4.BorderStyle = BorderStyle.None;

            ConfigureSectionLabel(label2, "Included Sources");
            ConfigureSectionLabel(label1, "Excluded Sources");

            panel3.Paint += (_, e) => PaintSectionPanel(e.Graphics, panel3.ClientRectangle);
            panel4.Paint += (_, e) => PaintSectionPanel(e.Graphics, panel4.ClientRectangle);

            panel3.Resize += (_, _) => LayoutListContainers();
            panel4.Resize += (_, _) => LayoutListContainers();

            LayoutListContainers();
        }

        private static void ConfigureSectionLabel(Label label, string text)
        {
            label.AutoSize = false;
            label.Text = text;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Font = new Font(label.Font.FontFamily, 8.25f, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(70, 78, 88);
            label.BackColor = Color.Transparent;
        }

        private void LayoutListContainers()
        {
            LayoutSectionContainer(panel3, label2, includeContainer, includeListPanel);
            LayoutSectionContainer(panel4, label1, flowLayoutPanel2, excludeListPanel);
        }

        private static void LayoutSectionContainer(
            Panel sectionPanel,
            Label label,
            Control placeholder,
            Control? activeListPanel)
        {
            label.SetBounds(
                8,
                0,
                Math.Max(0, sectionPanel.ClientSize.Width - 16),
                SectionHeaderHeightPx);

            var listBounds = new Rectangle(
                1,
                SectionHeaderHeightPx,
                Math.Max(0, sectionPanel.ClientSize.Width - 2),
                Math.Max(0, sectionPanel.ClientSize.Height - SectionHeaderHeightPx - 1));

            placeholder.Bounds = listBounds;

            if (activeListPanel != null)
                activeListPanel.Bounds = listBounds;

            sectionPanel.Invalidate();
        }

        private static void PaintSectionPanel(Graphics g, Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            using var headerBrush = new SolidBrush(Color.FromArgb(250, 250, 250));
            using var borderPen = new Pen(Color.FromArgb(210, 214, 218));
            using var headerLinePen = new Pen(Color.FromArgb(228, 231, 234));

            g.FillRectangle(
                headerBrush,
                0,
                0,
                bounds.Width,
                SectionHeaderHeightPx);

            g.DrawLine(
                headerLinePen,
                0,
                SectionHeaderHeightPx - 1,
                bounds.Width,
                SectionHeaderHeightPx - 1);

            g.DrawRectangle(
                borderPen,
                0,
                0,
                bounds.Width - 1,
                bounds.Height - 1);
        }
        public void SetUI(
            MEDIA_TYPE_ENUM mediaType,
            IReadOnlyList<MediaExportPriorityItemDTO> priorityDTOs,
            bool isScoring)
        {
            EnsureListPanels();

            scoringLabel.Visible = isScoring;

            this.mediaType = mediaType;

            string mediaTypeStr = mediaType.ToString();

            mediaItemUI.IconImageKey = mediaTypeStr;
            mediaItemUI.MediaDescription = mediaTypeStr.Replace("_", ": ");
            mediaItemUI.DatChipKey = string.Empty;

            includeListPanel!.ClearItems(true);
            excludeListPanel!.ClearItems(true);

            foreach (var dto in priorityDTOs)
            {
                if (dto.Include)
                    includeListPanel.AddItem(CreateItem(dto, false), false);
                else
                    excludeListPanel!.AddItem(CreateItem(dto, true), false);
            }

            includeListPanel.Relayout(false);
            excludeListPanel!.Relayout(false);
        }

        public IReadOnlyList<MediaExportPriorityItemDTO> GetIncludedPriorityItems()
        {
            EnsureListPanels();
            return includeListPanel!.GetDTOs();
        }

        public IReadOnlyList<MediaExportPriorityItemDTO> GetExcludedPriorityItems()
        {
            EnsureListPanels();
            return excludeListPanel!.GetDTOs();
        }

        private ExportPriorityItemUI CreateItem(MediaExportPriorityItemDTO dto, bool isExcluded)
        {
            var item = new ExportPriorityItemUI();
            item.SetUI(dto, 0, isExcluded);

            item.MoveUpRequested += MoveIncludedItemUp;
            item.MoveDownRequested += MoveIncludedItemDown;
            item.ExcludeRequested += ExcludeItem;
            item.IncludeRequested += IncludeItem;

            return item;
        }
        private sealed class RedrawScope : IDisposable
        {
            private const int WM_SETREDRAW = 0x000B;

            private readonly Control control;

            public RedrawScope(Control control)
            {
                this.control = control;

                if (control.IsHandleCreated)
                    _ = SendMessage(control.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            }

            public void Dispose()
            {
                if (!control.IsDisposed && control.IsHandleCreated)
                {
                    _ = SendMessage(control.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                    control.Invalidate(true);
                    control.Update();
                }
            }

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        }
        private void MoveIncludedItemUp(ExportPriorityItemUI item)
        {
            EnsureListPanels();

            using var scrollGuard = new ScrollPositionGuard(this);
            includeListPanel!.MoveItemUp(item);
        }

        private void MoveIncludedItemDown(ExportPriorityItemUI item)
        {
            EnsureListPanels();

            using var scrollGuard = new ScrollPositionGuard(this);
            includeListPanel!.MoveItemDown(item);
        }

        private void ExcludeItem(ExportPriorityItemUI item)
        {
            EnsureListPanels();

            if (!includeListPanel!.ContainsItem(item))
                return;

            using var redrawGuard = new RedrawScope((Control?)FindForm() ?? this);
            using var scrollGuard = new ScrollPositionGuard(this);

            includeListPanel.RemoveItem(item, false);
            item.SetExcluded(true);
            excludeListPanel?.InsertItem(0, item, true);
        }

        private void IncludeItem(ExportPriorityItemUI item)
        {
            EnsureListPanels();

            if (!excludeListPanel!.ContainsItem(item))
                return;

            using var redrawGuard = new RedrawScope((Control?)FindForm() ?? this);
            using var scrollGuard = new ScrollPositionGuard(this);

            excludeListPanel.RemoveItem(item, false);
            item.SetExcluded(false);
            includeListPanel!.InsertItem(0, item, true);
        }

        private void EnsureListPanels()
        {
            includeListPanel ??= ReplaceFlowPanel(includeContainer);
            excludeListPanel ??= ReplaceFlowPanel(flowLayoutPanel2);

            LayoutListContainers();
        }
        private static AnimatedExportListPanelUI ReplaceFlowPanel(FlowLayoutPanel placeholder)
        {
            var parent = placeholder.Parent ?? throw new InvalidOperationException("Placeholder has no parent.");

            var panel = new AnimatedExportListPanelUI
            {
                Location = placeholder.Location,
                Size = placeholder.Size,
                Anchor = placeholder.Anchor,
                BackColor = Color.White
            };

            placeholder.Visible = false;
            parent.Controls.Add(panel);
            panel.BringToFront();

            return panel;
        }

        private sealed class ScrollPositionGuard : IDisposable
        {
            private readonly List<(ScrollableControl Control, Point Position)> snapshots = [];

            public ScrollPositionGuard(Control start)
            {
                Control? current = start;

                while (current != null)
                {
                    if (current is ScrollableControl scrollable && scrollable.AutoScroll)
                        snapshots.Add((scrollable, scrollable.AutoScrollPosition));

                    current = current.Parent;
                }
            }

            public void Dispose()
            {
                Restore();

                if (snapshots.Count == 0)
                    return;

                var first = snapshots[0].Control;

                if (!first.IsDisposed && first.IsHandleCreated)
                    first.BeginInvoke(new Action(Restore));
            }

            private void Restore()
            {
                foreach (var snapshot in snapshots)
                {
                    if (snapshot.Control.IsDisposed)
                        continue;

                    snapshot.Control.AutoScrollPosition = new Point(
                        -snapshot.Position.X,
                        -snapshot.Position.Y);
                }
            }
        }
    }
}