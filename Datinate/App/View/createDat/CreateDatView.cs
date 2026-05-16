using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using Datinate.Shared;

namespace com.RADIO.Datinate.App.View.createDat
{
    public partial class CreateDatView : UserControl, ICreateDatView 
    {
        public event Action<DatVO, string, bool>? CreateDatEvt;

        private DatVO? vo;
        private string? fileFolderPath = null;

        private RomSummaryManager? romSummaryManager;

        public CreateDatView() 
        {
            InitializeComponent();

            UIHelper.PopButton(createBtn);
            Facade.RegisterActor(this);
        }

        public void SetView(DatVO vo) 
        {
            this.vo = vo;

            ClearView();

            if (vo == null)
                return;

            nameTextBox.Text = vo.DatHeaderVO.Name;
            filenameTextBox.Text = vo.DatHeaderVO.Name;
            descriptionTextBox.Text = vo.DatHeaderVO.Description;
            categoryTextBox.Text = vo.DatHeaderVO.Category;
            versionTextBox.Text = vo.DatHeaderVO.Version;
            dateTextBox.Text = DateTime.Today.ToShortDateString();
            versionTextBox.Text = vo.DatHeaderVO.Version;
            commentTextBox.Text = vo.DatHeaderVO.Comment;

            romSummaryManager = new RomSummaryManager();

            foreach (var gameVO in vo.Entries)
            {
                romSummaryManager.manageRoms(gameVO);
            }


            RomExt[] roms = romSummaryManager.getSummaries();

            ListViewItem item;
            for (int i = 0; i < roms.Length; i++) {

                if (roms[i].getExtension().Equals(Constants.NO_VALUE))
                    continue;

                item = new ListViewItem("." + roms[i].getExtension());
                item.Checked = true;
                combo_extensions.Items.Add(item);

            }
        }

        public void ClearView() 
        {
            nameTextBox.Text = "";
            filenameTextBox.Text = "";
            descriptionTextBox.Text = "";
            categoryTextBox.Text = "";
            versionTextBox.Text = "";
            dateTextBox.Text = "";
            versionTextBox.Text = "";
            commentTextBox.Text = "";

            useMachineTagsCB.Checked = true;

            checkbox_selectAllExt.Checked = true;
            combo_extensions.Enabled = false;
            combo_extensions.Items.Clear();

            romSummaryManager = null;
        }

        private DatVO? GetDatVO() {

            if (vo == null)
                return null;

            List<string> extensionsList = new List<string>();

            for (int i = 0; i < combo_extensions.Items.Count; i++) 
            {
                if (combo_extensions.Items[i].Checked) 
                {
                    extensionsList.Add(combo_extensions.Items[i].Text);
                }
            }
            string[] extensions = extensionsList.ToArray();

            var entries = new List<DatGameVO>();

            foreach (var gameVO  in vo.Entries)
            {
                var filteredRoms = new List<DatRomVO>();

                for (int j = 0; j < gameVO.Roms.Length; j++) 
                {
                    var romVO = gameVO.Roms[j];

                    for (int k = 0; k < extensions.Length; k++) 
                    {
                        var ext = extensions[k];
                        if (romVO.Name.EndsWith(ext, true, null)) 
                        {
                            filteredRoms.Add(romVO);
                            break;
                        }
                    }
                }

                entries.Add(
                    new DatGameVO(
                        gameVO.Name, 
                        gameVO.Description,
                        gameVO.Publisher, 
                        gameVO.Region, 
                        gameVO.Date,
                        gameVO.Category,
                        gameVO.MameLaunchName, 
                        gameVO.ParentName, 
                        filteredRoms.ToArray(),
                        gameVO.PartOwnerName)
                );
            }

            DatVO newDatVO = new DatVO(
                vo.DatFullpath,
                vo.DatHeaderVO.Clone(),
                entries,
                vo.RawDatText);

            return newDatVO;
        }

        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        private bool GetUseMachineTags() 
        {
            return useMachineTagsCB.Checked;
        }

        private void createBtn_Click(object sender, EventArgs e) 
        {
            if (string.IsNullOrWhiteSpace(browseSaveFolderDialog.SelectedPath)) 
            {
                MessageBox.Show("Please select a folder to save the DAT file.", "There was a problem");
                return;
            }

            if (string.IsNullOrWhiteSpace(filenameTextBox.Text)) 
            {
                MessageBox.Show("Please enter a filename to proceed.", "There was a problem");
                return;
            }

            char[] invalidFileChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < filenameTextBox.Text.Length; i++) 
            {
                char character = filenameTextBox.Text[i];
                if (character == '.') 
                {
                    MessageBox.Show("Do not include file extensions (or any fullstops) in the filename.", "There was a problem");
                    return;
                }

                for (int j = 0; j < invalidFileChars.Length; j++) 
                {
                    if (character == invalidFileChars[j]) 
                    {
                        MessageBox.Show("The filename contains one or more invalid characters.", "There was a problem");
                        return;
                    }
                }
            }
            var datVO = GetDatVO();
                 
            if (datVO != null && !string.IsNullOrWhiteSpace(fileFolderPath) && !string.IsNullOrWhiteSpace(filenameTextBox.Text)) 
            {
                var fullpath = Path.Combine(fileFolderPath, filenameTextBox.Text + ".dat");
                CreateDatEvt?.Invoke(datVO, fullpath, GetUseMachineTags());
            }
        }

        private void selectFolderBtn_Click(object sender, EventArgs e) 
        {
            DialogResult result = browseSaveFolderDialog.ShowDialog();

            if (result == DialogResult.OK) 
            {
                fileFolderPath = folderDirectoryLabel.Text = browseSaveFolderDialog.SelectedPath;
            }
        }

        private void checkbox_selectAllExt_CheckedChanged(object sender, EventArgs e) 
        {
            if (checkbox_selectAllExt.Checked) 
            {
                combo_extensions.Enabled = false;
                btn_deselectAll.Enabled = false;

                if (combo_extensions.Items != null) 
                {
                    for (int i = 0; i < combo_extensions.Items.Count; i++) 
                    {
                        combo_extensions.Items[i].Checked = true;
                    }
                }
            }
            else 
            {
                combo_extensions.Enabled = true;
                btn_deselectAll.Enabled = true;
            }
        }

        private void btn_deselectAll_Click(object sender, EventArgs e) 
        {
            if (combo_extensions.Items != null) 
            {
                for (int i = 0; i < combo_extensions.Items.Count; i++) 
                {
                    combo_extensions.Items[i].Checked = false;
                }
            }
        }
    }
}
