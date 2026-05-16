using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using Datinate.Shared;
using System.Diagnostics;

namespace datinate.app
{
    public partial class DatSummaryView : UserControl, IDatSummaryView 
    {

        public event Action<DatSummaryVO>? DatSelectedEvt;
        public event Action? HomeClickEvt;

        // NOTE: local references kept because we can filter this information:
        DatSummaryVO[] summaries = new DatSummaryVO[] { };
        DatSummaryVO? selectedSummary = null;
        UnitFormatHelper.Unit unit = UnitFormatHelper.Unit.MB;
        bool showUnitInCells;

        // NOTE: As above - we need this local store in case units are updated:
        ulong totalSize;
        int gameCount;
        int romCount;
        int datCount;

        public DatSummaryView() 
        {
            InitializeComponent();

            Facade.RegisterActor(this);
            listView.AddColumns(
                new ColumnHeaderVO.NameEnum[] {
                    ColumnHeaderVO.NameEnum.Folder
                    , ColumnHeaderVO.NameEnum.DAT
                    , ColumnHeaderVO.NameEnum.Version
                    , ColumnHeaderVO.NameEnum.Size
                    , ColumnHeaderVO.NameEnum.Entries
                    , ColumnHeaderVO.NameEnum.ROMs
                });

            filterUI.filterEvt += new EventHandler(OnFilterEvt);
            filterUI.clearEvt += new EventHandler(OnClearFilterEvt);
            ClearView();
        }
        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        /**
         * Populate table with correct unit info.
         * We also CLEAR current item list
         */
        public void PopulateTable(DatSummaryVO[] summaries, UnitFormatHelper.Unit unit, bool showUnitInCell) {
            Ui(() => 
            { 
                PopulateTable(summaries, unit, showUnitInCell, null);
            });
        }

        /**
         * Pass NULL for filter to AVOID all filtering
:         */
        private void PopulateTable(
            DatSummaryVO[] summaries, 
            UnitFormatHelper.Unit unit, 
            bool showUnitInCell, 
            string? filter) 
        {
            bool autoResize = (listView.Items.Count == 0);
            listView.Items.Clear();
            this.Enabled = true;
            this.summaries = summaries;

            // NOTE: we must reset values so we aren't forever accumulating (when filtering or loading afresh):
            totalSize = 0;
            gameCount = 0;
            romCount = 0;
            datCount = 0;

            // NOTE: DO NOT remove this line or we get index out of range error:
            this.listView.ListViewItemSorter = null;

            for (int i = 0; i < summaries.Length; i++) 
            {
                var summary = summaries[i];
                var header = summary.DatHeader;

                if (filter != null) {
                    if (!header.Name.ToLower().Contains(filter))
                        continue;
                }

                datCount++;
                totalSize += summary.SizeTotal;
                romCount += summary.RomsTotal;
                gameCount += summary.GamesTotal;

                listView.AddRow(
                    new RowVO[] {
                        new RowVO(ColumnHeaderVO.NameEnum.Folder, summary.Folder)
                        , new RowVO(ColumnHeaderVO.NameEnum.DAT, header.Name)
                        , new RowVO(ColumnHeaderVO.NameEnum.Version, header.Version)
                        , new RowVO(ColumnHeaderVO.NameEnum.Size, summary.SizeTotal.ToString())
                        , new RowVO(ColumnHeaderVO.NameEnum.Entries, summary.GamesTotal.ToString())
                        , new RowVO(ColumnHeaderVO.NameEnum.ROMs, summary.RomsTotal.ToString())}
                    , summary);
            } // end for i

            UpdateUnits(unit, showUnitInCell, autoResize);
        }

        public void HighlightDats(
            DatGrouperProjectEntry[] includeDatHeadlines, 
            DatGrouperProjectEntry[] ignoreDatHeadlines) 
        {
            Ui(() =>
            {
                UnhighlightDats();

                HighlightDats(
                    includeDatHeadlines
                    , DatFilterHelper.GetExpressionColour(DatFilterHelper.EXPRESSION_ACTION_ENUM.INCLUDE)
                );
                HighlightDats(
                    ignoreDatHeadlines
                    , DatFilterHelper.GetExpressionColour(DatFilterHelper.EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL)
                );
            });
        }

        private void HighlightDats(DatGrouperProjectEntry[] datHeadlineVOs, Color c) 
        {
            foreach (ListViewItem item in listView.Items) 
            {
                DatSummaryVO? summary = item.Tag as DatSummaryVO;

                if (summary == null)
                    continue;

                for (int i = 0; i < datHeadlineVOs.Length; i++) 
                {
                    if (datHeadlineVOs[i].DatFullpath == summary.DatFullpath) {
                        item.ForeColor = c;
                        break;
                    }
                } // end for i
            }
        }

        private void UnhighlightDats() 
        {
            foreach (ListViewItem item in listView.Items) 
            {
                item.ForeColor = Color.Black;
            }
        }
       
        /**
         * Update the label at bottom of table.
         */
        private void UpdateSummaryLabel() 
        {
            // NOTE: this method may be called when it is empty, so watch for null pointers:
            if (summaries == null || summaries.Length == 0)
                return;

            string sizeStr = UnitFormatHelper.Convert(totalSize, unit, showUnitInCells);
            string result;

            result = "■ Total Size: " + 
                sizeStr + "    ■ Entries: " + 
                DatinateHelper.GetReadableNumber(gameCount) + 
                "    ■ ROMs: " +
                DatinateHelper.GetReadableNumber(romCount) + 
                "    ■ DATs: " +
                DatinateHelper.GetReadableNumber(datCount);

            summaryLabel.Text = result;
        }

        /**
         * Sort data by column header.
         * NOTE: switching between ascending and descending hasn't been looked at properly:
         */
        private void onColumnClick(object o, ColumnClickEventArgs e)
        {
            ColumnHeaderVO? c = listView.Columns[e.Column].Tag as ColumnHeaderVO;

            if (c == null)
            {
                Debug.Print(this + ": Error: onColumnClick() Tag is not a known VO.");
                return;
            }

            c.switchSorting();
            this.listView.ListViewItemSorter = c.getSorter(e.Column);
            
        }
        public void UpdateUnits(UnitFormatHelper.Unit unit, bool showUnitInCells, bool autoResize)
        {
            Ui(() =>
            {
                this.unit = unit;
                this.showUnitInCells = showUnitInCells;

                if (IsDisposed || !IsHandleCreated)
                    return;

                listView.BeginUpdate();
                try
                {
                    var oldSorter = listView.ListViewItemSorter;
                    listView.ListViewItemSorter = null;

                    try
                    {
                        var items = new ListViewItem[listView.Items.Count];
                        listView.Items.CopyTo(items, 0);

                        for (int i = 0; i < items.Length; i++)
                        {
                            var item = items[i];

                            var summary = item.Tag as DatSummaryVO;
                            if (summary == null)
                                continue;

                            var result = UnitFormatHelper.Convert(summary.SizeTotal, unit, showUnitInCells);
                            listView.SetRowSubItemText(item, ColumnHeaderVO.NameEnum.Size, result);
                        }

                        listView.SetColumnText(ColumnHeaderVO.NameEnum.Size, UnitFormatHelper.getColumnText(unit));

                        if (autoResize)
                            listView.ResizeColumnsToBestFit();

                        UpdateSummaryLabel();
                    }
                    finally
                    {
                        listView.ListViewItemSorter = oldSorter;
                        if (oldSorter != null)
                            listView.Sort();
                    }
                }
                finally
                {
                    listView.EndUpdate();
                }
            });
        }
        /**
         * Clicked on DAT - load details:
         */
        private void OnDatSelected(object sender, ListViewItemSelectionChangedEventArgs e) 
        {

            if (listView.SelectedItems.Count == 0)
                return;

            if (listView.SelectedItems[0].Tag is not DatSummaryVO)
                return;
            else
                selectedSummary = (listView.SelectedItems[0].Tag as DatSummaryVO);

            if (selectedSummary == null)
                return;

            DatSelectedEvt?.Invoke(selectedSummary);
        }

        private void OnClearFilterEvt(object? sender, EventArgs e) 
        {
            PopulateTable(summaries, unit, showUnitInCells, null);
        }

        private void OnFilterEvt(object? sender, EventArgs e) 
        {
            PopulateTable(summaries, unit, showUnitInCells, filterUI.GetFilterValue());
        }

        public void ClearView() 
        {
            Ui(() =>
            {
                summaryLabel.Text = "";
                summaryLabel.Refresh();
                listView.Items.Clear();

                this.summaries = new DatSummaryVO[] { };
                filterUI.Empty();
            });
        }

        private void homeBtn_Click(object? sender, EventArgs e) 
        {
            HomeClickEvt?.Invoke();
        }

        private void Ui(Action action)
        {
            if (IsDisposed)
                return;

            if (!IsHandleCreated)
            {
                void handler(object? s, EventArgs e)
                {
                    HandleCreated -= handler;
                    Ui(action);
                }

                HandleCreated += handler;
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(action);
                return;
            }

            action();
        }
    }
}
