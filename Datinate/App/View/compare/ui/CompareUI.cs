using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using System.Diagnostics;

namespace datinate.app
{
    public partial class CompareUI : UserControl 
    {
        public EventHandler? CustomiseClickEvt;
        public EventHandler? CreateDatClickEvt;

        public enum UiPosition {
            Left
            , Middle
            , Right
        }

        private UnitFormatHelper.Unit unit;
        private bool showUnitInCells;

        private DatVO? datVO;

        private UiPosition uiPosition;

        public CompareUI() 
        {
            InitializeComponent();
            UIHelper.PopSplitter(splitContainer3);

            gameList.AddColumns(
                new ColumnHeaderVO.NameEnum [] {
                    ColumnHeaderVO.NameEnum.Entry
                    , ColumnHeaderVO.NameEnum.Description
                    , ColumnHeaderVO.NameEnum.Size
                    , ColumnHeaderVO.NameEnum.ROMs
                });

            summaryList.AddColumns(
                new ColumnHeaderVO.NameEnum [] {
                    ColumnHeaderVO.NameEnum.Ext
                    , ColumnHeaderVO.NameEnum.ROMs
                    , ColumnHeaderVO.NameEnum.Size
                    , ColumnHeaderVO.NameEnum.Size_PC
                    , ColumnHeaderVO.NameEnum.Entries
                });
            summaryList.SetColumnText(ColumnHeaderVO.NameEnum.Size_PC, "Size (%)");

            ClearUI();
        }

        public DatVO? GetDatVO() => datVO;
        
        public void SetUI(DatVO datVO, UiPosition uiPosition) 
        {
            SetUIInternal(datVO, uiPosition);
        }
        public void SetUIInternal(DatVO datVO, UiPosition uiPosition) 
        {
            bool autoResize = (gameList.Items.Count == 0);

            ClearUI();

            
            this.gameList.ListViewItemSorter = null;
            this.summaryList.ListViewItemSorter = null;

            this.uiPosition = uiPosition;
            this.datVO = datVO;

            
            SetNameLabel();
            
            RomSummaryManager manager = SetRomList(null);
            SetRomSummaryList(manager);

            UpdateUnits(unit, showUnitInCells);

            Enabled = true;
        }

        public void ClearUI() 
        {
            datVO = null;
            gameList.Items.Clear();
            summaryList.Items.Clear();

            romSummaryLabel.Text = "";
            nameLabel.Text = "---";
            romListSummary.Text = "";
            
            Enabled = false;
        }

        public void UpdateUnits(UnitFormatHelper.Unit unit, bool showUnitInCells) 
        {
            this.unit = unit;
            this.showUnitInCells = showUnitInCells;

            UpdateListView(gameList);
            UpdateListView(summaryList);
        }

        protected void UpdateListView(CustomDetailsListView listView)
        {
            listView.BeginUpdate();
            try
            {
                for (int i = 0; i < listView.Items.Count; i++)
                {
                    ListViewItem item = listView.Items[i];

                    ulong? totalSize = null;

                    if (item.Tag is IByteReporter byteReporter)
                        totalSize = byteReporter.GetTotalSize();
                    else if (item.Tag is RomExt romExt)
                        totalSize = romExt.getTotalSize();

                    if (totalSize == null)
                        continue;

                    string result = UnitFormatHelper.Convert(totalSize.Value, unit, showUnitInCells);
                    listView.SetRowSubItemText(item, ColumnHeaderVO.NameEnum.Size, result);
                }

                listView.SetColumnText(ColumnHeaderVO.NameEnum.Size, UnitFormatHelper.getColumnText(unit));
            }
            finally
            {
                listView.EndUpdate();
            }
        }
        protected RomSummaryManager SetRomList(string filter) 
        {
            // NOTE: datVO.datSummaryVO may be NULL.

            RomSummaryManager romSummaryManager = new RomSummaryManager();

            ulong totalSize = 0;

            int totalRomCount = 0;

            if (datVO != null)
            {
                foreach (var game in datVO.Entries)
                {
                    if (filter != null)
                    {
                        if (!game.Name.ToLower().Contains(filter))
                            continue;
                    }

                    totalSize += game.GetTotalSize();
                    totalRomCount += game.Roms.Length;

                    gameList.AddRow(
                        new RowVO[] {
                        new RowVO(ColumnHeaderVO.NameEnum.Entry, DatinateHelper.GetDisplayNameWithPartOwner(game))
                        , new RowVO(ColumnHeaderVO.NameEnum.Description, game.Description ?? string.Empty)
                        , new RowVO(ColumnHeaderVO.NameEnum.Size, game.GetTotalSize().ToString())
                        , new RowVO(ColumnHeaderVO.NameEnum.ROMs, game.Roms.Length.ToString())
                        }
                        , game);


                    romSummaryManager.manageRoms(game);
                }
            }

            // TODO: Update unit display dynamically?
            string sizeStr = UnitFormatHelper.Convert(totalSize, unit, showUnitInCells);

            var games = DatinateHelper.GetReadableNumber(datVO?.Entries.Count ?? 0);
            var roms = DatinateHelper.GetReadableNumber(totalRomCount);
            
            romListSummary.Text = "Entries: " + games + " │ ROMs: " + roms + " │ Size: " + sizeStr + "";
            
            return romSummaryManager;
        }

        protected void SetRomSummaryList(RomSummaryManager romSummaryManager) 
        {
            bool autoResize = (summaryList.Items.Count == 0);
            summaryList.Items.Clear();
            summaryList.Refresh();

            RomExt [] romExts = romSummaryManager.getSummaries();

            ulong datSize = datVO?.SizeTotal ?? 0;

            for (int i = 0; i < romExts.Length; i++) {
                RomExt romExt = romExts[i];
                summaryList.AddRow(
                    new RowVO[] {
                        new RowVO(ColumnHeaderVO.NameEnum.Ext, romExt.getExtension())
                        , new RowVO(ColumnHeaderVO.NameEnum.ROMs, romExt.getRomOccurances().ToString())
                        , new RowVO(ColumnHeaderVO.NameEnum.Size, romExt.getTotalSize().ToString())
                        , new RowVO(ColumnHeaderVO.NameEnum.Size_PC, romExt.getPercentage(datSize).ToString("0.00") + " %")
                        , new RowVO(ColumnHeaderVO.NameEnum.Entries, romExt.getGameOccurances().ToString())
                    }
                    , romExt);
            } // end for i

            if (romExts.Length == 1)
                romSummaryLabel.Text = "1 ROM type found";
            else
                romSummaryLabel.Text = romExts.Length + " ROM types found";

        }

        protected void SetNameLabel() 
        {   
            switch(uiPosition) 
            {
                case UiPosition.Left:
                    nameLabel.Text = datVO?.GetDatNameWithoutExt() ?? string.Empty;
                    break;
                case UiPosition.Right:
                    nameLabel.Text = datVO?.GetDatNameWithoutExt() ?? string.Empty;// + ": "; //"Right DAT: ";
                    break;
                case UiPosition.Middle:
                    nameLabel.Text = "Matches";
                    return;
            }
        }

        /**
         * Sort a list view based on column click
         */
        private void onColumnClick(object o, ColumnClickEventArgs e)
        {

            if ((o as ListView) == null)
                return;

            ListView? listView = o as ListView;


            if (listView == null ||  (listView.Columns[e.Column].Tag as ColumnHeaderVO) == null)
            {
                Debug.Print(this + ": Error: onColumnClick() Tag is not a known VO.");
                return;
            }

            ColumnHeaderVO? c = listView.Columns[e.Column].Tag as ColumnHeaderVO;

            if (c != null)
            {
                c.switchSorting();
                listView.ListViewItemSorter = c.getSorter(e.Column);
            }
        }

        private void onCustomise(object sender, EventArgs e) 
        {
            if (CustomiseClickEvt != null)
                CustomiseClickEvt(this, new EventArgs());
        }

        private void createDatBtn_Click(object sender, EventArgs e) 
        {
            if (CreateDatClickEvt != null)
                CreateDatClickEvt(this, new EventArgs());
        }
    }
}
