using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using Datinate.Shared;
using System.Diagnostics;

namespace datinate.app
{
    public partial class DatDetailsView : UserControl, IDatDetailsView
    {
        public event Action<DatVO>? CustomiseClickEvt;
        public event Action? CompareLeftEvt;
        public event Action? CompareRightEvt;
        public event Action? ReadDatInDefaultAppFailEvt;
        public event Action<DatVO>? ShowAddToProjectEvt;

        DatVO? datVO;
        UnitFormatHelper.Unit unit;
        bool showUnitInCells;
        DatGameVO? currentGameVO;
        protected RomSummaryManager? romSummaryManager;

        public DatDetailsView()
        {
            InitializeComponent();

            gameList.AddColumns(
                new ColumnHeaderVO.NameEnum[] {
                    ColumnHeaderVO.NameEnum.Entry
                    , ColumnHeaderVO.NameEnum.Size
                    , ColumnHeaderVO.NameEnum.ROMs
                    , ColumnHeaderVO.NameEnum.Description});

            romSummaryList.AddColumns(
                new ColumnHeaderVO.NameEnum[] {
                    ColumnHeaderVO.NameEnum.Ext
                    , ColumnHeaderVO.NameEnum.ROMs
                    , ColumnHeaderVO.NameEnum.Size
                    , ColumnHeaderVO.NameEnum.Size_PC
                    , ColumnHeaderVO.NameEnum.Entries
                });
            
            romSummaryList.SetColumnText(ColumnHeaderVO.NameEnum.Size_PC, "Size (%)");

            UIHelper.PopSplitter(splitContainer);

            compareLeftBtn.Enabled = false;
            compareRightBtn.Enabled = false;
            openDatBtn.Enabled = false;
            customiseBtn.Enabled = false;
            sendToDatGrouperBtn.Enabled = false;

            filterUI.filterEvt += new EventHandler(OnFilter);
            filterUI.clearEvt += new EventHandler(OnClearFilter);
            ClearView();

            Facade.RegisterActor(this);
        }
        public void SetView(
            DatVO datVO
            , UnitFormatHelper.Unit unit
            , bool showUnitInCells)
        {
            SetView(datVO, unit, showUnitInCells, null);
        }
        public void ClearView()
        {
            Ui(() =>
            {
                datDetailsLabel.Text = "Selected DAT Details";
                gameDetailsLabel.Text = "Selected Game Details";
                gameList.Items.Clear();
                romSummaryList.Items.Clear();
                datTreeView.Nodes.Clear();
                this.currentGameVO = null;

                filterUI.Empty();

                romSummaryLabel.Text = "No ROMs Loaded";
            });
        }
        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        private void OnFilter(object? sender, EventArgs e)
        {
            SetView(datVO, unit, showUnitInCells, filterUI.GetFilterValue());
        }
        private void OnClearFilter(object? sender, EventArgs e)
        {
            SetView(datVO, unit, showUnitInCells, null);
        }

        /**
         * Pass NULL for filter to AVOID all filtering
         */
        protected void SetView(
            DatVO? datVO
            , UnitFormatHelper.Unit unit
            , bool showUnitInCells
            , string? filter)
        {
            if (datVO == null) return;
            Ui(() =>
            {
                // NOTE: DO NOT remove this line or we get index out of range error:
                this.gameList.ListViewItemSorter = null;
                this.romSummaryList.ListViewItemSorter = null;

                this.datVO = datVO;

                this.Enabled = true;
                bool autoResize = (gameList.Items.Count == 0);

                currentGameVO = null;
                gameList.Items.Clear();
                gameList.Refresh();
                datTreeView.Nodes.Clear();

                datDetailsLabel.Text = "DAT: " + datVO.DatHeaderVO.Name;
                gameDetailsLabel.Text = "";

                this.unit = unit;
                this.showUnitInCells = showUnitInCells;

                // NOTE: Update our list views:
                romSummaryManager = SetGameList(filter);

                if (romSummaryManager != null)
                    SetRomSummaryList(romSummaryManager);

                UpdateUnits(unit, showUnitInCells);

                tabControl.SelectedIndex = 0;

                compareLeftBtn.Enabled = true;
                compareRightBtn.Enabled = true;
                openDatBtn.Enabled = true;
                customiseBtn.Enabled = true;
                sendToDatGrouperBtn.Enabled = true;
            });
        }

        /**
         * Sets the game list.
         * To save repeating ourselves, we also return a ROM summary 
         * ready for setting the next list:
         */
        protected RomSummaryManager? SetGameList(string? filter)
        {
            if (datVO == null) return null;

            romSummaryManager = new RomSummaryManager();

            List<string> roms = new List<string>();

            foreach (var game in datVO.Entries)
            {
                if (filter != null)
                {
                    if (!game.Name.ToLower().Contains(filter))
                        continue;
                }

                string entryName = game.Name;
                if (!string.IsNullOrWhiteSpace(game.PartOwnerName))
                    entryName += $" <{game.PartOwnerName}>";

                gameList.AddRow(
                    new RowVO[] {
                        new RowVO(ColumnHeaderVO.NameEnum.Entry, DatinateHelper.GetDisplayNameWithPartOwner(game))
                        , new RowVO(ColumnHeaderVO.NameEnum.Size, game.GetTotalSize().ToString())
                        , new RowVO(ColumnHeaderVO.NameEnum.ROMs, game.Roms.Length.ToString())
                        , new RowVO(ColumnHeaderVO.NameEnum.Description, game.Description ?? string.Empty)
                    }
                    , game);

                for (int j = 0; j < game.Roms.Length; j++)
                {
                    roms.Add(game.Roms[j].Sha1);
                }

                romSummaryManager.manageRoms(game);
            } // end for i

            Debug.Print("************************************************************");
            Debug.Print("Total Games : " + roms.Count);
            Debug.Print("Unique ROMs: " + roms.Distinct().Count());
            Debug.Print("************************************************************");

            return romSummaryManager;
        }

        protected void SetRomSummaryList(RomSummaryManager romSummaryManager)
        {
            bool autoResize = (romSummaryList.Items.Count == 0);
            romSummaryList.Items.Clear();
            romSummaryList.Refresh();

            RomExt[] romExts = romSummaryManager.getSummaries();

            if (datVO == null) return;

            ulong datSize = datVO.SizeTotal;

            string readableSize;

            for (int i = 0; i < romExts.Length; i++)
            {
                RomExt romExt = romExts[i];

                readableSize = UnitFormatHelper.Convert(romExt.getTotalSize(), unit, showUnitInCells);

                romSummaryList.AddRow(
                    new RowVO[] {
                        new RowVO(ColumnHeaderVO.NameEnum.Ext, romExt.getExtension())
                        , new RowVO(ColumnHeaderVO.NameEnum.ROMs, romExt.getRomOccurances().ToString())
                        , new RowVO(ColumnHeaderVO.NameEnum.Size, readableSize)
                        , new RowVO(ColumnHeaderVO.NameEnum.Size_PC, romExt.getPercentage(datSize).ToString("0.00") + " %")
                        , new RowVO(ColumnHeaderVO.NameEnum.Entries, romExt.getGameOccurances().ToString())
                    }
                    , romExt);
            }

            if (romExts.Length == 1)
                romSummaryLabel.Text = "1 ROM type found";
            else
                romSummaryLabel.Text = romExts.Length + " ROM types found";

        }

        /**
         * Update tree view:
         */
        protected void ShowGameBreakdown(DatGameVO datGameVO)
        {
            gameDetailsLabel.Text = "Game: " + datGameVO.Name;

            this.currentGameVO = datGameVO;
            datTreeView.Nodes.Clear();

            TreeNode gameNode = datTreeView.Nodes.Add(datGameVO.Name);
            gameNode.Tag = datGameVO;

            gameNode.Nodes.Add("Description").Nodes.Add(datGameVO.Description);

            string result = UnitFormatHelper.Convert(datGameVO.GetTotalSize(), unit, showUnitInCells);

            gameNode.Nodes.Add("Size").Nodes.Add(result);

            TreeNode romSetTree = gameNode.Nodes.Add("ROM");

            for (int j = 0; j < datGameVO.Roms.Length; j++)
            {
                DatRomVO datRomVO = datGameVO.Roms[j];
                TreeNode romTree = romSetTree.Nodes.Add(datRomVO.Name);

                string romResult = UnitFormatHelper.Convert(datRomVO.Size, unit, showUnitInCells);

                romTree.Nodes.Add("Size").Nodes.Add(romResult);

                TreeNode checksumTree = romTree.Nodes.Add("Checksum");
                checksumTree.Nodes.Add("SHA1").Nodes.Add(datRomVO.Sha1);
                checksumTree.Nodes.Add("MD5").Nodes.Add(datRomVO.Md5);
                checksumTree.Nodes.Add("CRC").Nodes.Add(datRomVO.Crc);
            }

            datTreeView.ExpandAll();

            // scroll to the top:
            if (datTreeView.Nodes.Count > 0)
                datTreeView.Nodes[0].EnsureVisible();

            tabControl.SelectedIndex = 1;
        }

        /**
         * Sort a list view based on column click
         */
        private void OnColumnClick(object o, ColumnClickEventArgs e)
        {

            if ((o as ListView) == null)
                return;

            ListView? listView = o as ListView;
            if (listView == null) return;

            if ((listView.Columns[e.Column].Tag as ColumnHeaderVO) == null)
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

        /**
         * Change how units are presented throughout view
         */
        public void UpdateUnits(UnitFormatHelper.Unit unit, bool showUnitInCells)
        {
            Ui(() =>
            {
                this.unit = unit;
                this.showUnitInCells = showUnitInCells;

                UpdateListView(gameList);

                if (romSummaryManager != null)
                    SetRomSummaryList(romSummaryManager);

                UpdateListView(romSummaryList);

                if (currentGameVO != null)
                    ShowGameBreakdown(currentGameVO);
            });
        }

        protected void UpdateListView(CustomDetailsListView listView)
        {
            listView.BeginUpdate();
            try
            {
                for (int i = 0; i < listView.Items.Count; i++)
                {
                    ListViewItem item = listView.Items[i];
                    IByteReporter? thing = item.Tag as IByteReporter;

                    if (thing == null)
                        continue;

                    string result = UnitFormatHelper.Convert(thing.GetTotalSize(), unit, showUnitInCells);
                    listView.SetRowSubItemText(item, ColumnHeaderVO.NameEnum.Size, result);
                }

                listView.SetColumnText(ColumnHeaderVO.NameEnum.Size, UnitFormatHelper.getColumnText(unit));
            }
            finally
            {
                listView.EndUpdate();
            }
        }

        private void OnGameSelectionChange(object sender, ListViewItemSelectionChangedEventArgs e)
        {

            if (gameList.SelectedItems.Count == 0)
                return;

            var tag = gameList.SelectedItems[0].Tag as DatGameVO;
            
            if (tag != null)
                ShowGameBreakdown(tag);
        }

        private void OnCompareClick(object sender, System.EventArgs e)
        {
            if (sender == compareLeftBtn)
            {
                CompareLeftEvt?.Invoke();
            }
            else if (sender == compareRightBtn)
            {
                CompareRightEvt?.Invoke();
            }
        }

        private void OnCustomiseClick(object sender, EventArgs e)
        {
            if (datVO != null)
                CustomiseClickEvt?.Invoke(datVO);
        }

        private void OpenDatBtn_Click(object sender, EventArgs e)
        {
            if (datVO != null)
            {
                var problem = false;

                try
                {
                    var P = new Process();
                    P.StartInfo.FileName = datVO.DatFullpath;
                    P.StartInfo.Verb = "";
                    if (P.Start())
                    {
                        Debug.WriteLine("--> result: App start OK");
                    }
                    else
                    {
                        Debug.WriteLine("--> result: App start FAIL");
                        problem = true;
                    }

                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                    problem = true;
                }

                // Open DAT directory instead if file association is not set:
                if (problem)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo { FileName = datVO.DatFullpath, UseShellExecute = true });
                    }
                    catch (Exception)
                    {
                        //Console.WriteLine("--> Attempt Open Folder??? " +error.ToString());
                        //Console.WriteLine("--> a: " + Path.GetDirectoryName(datVO.datPath));
                    }

                }
            } // end if (datVO != null)
        }

        private void AddToProjectBtn_Click(object sender, EventArgs e)
        {
            if (datVO != null)
                ShowAddToProjectEvt?.Invoke(datVO);
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
