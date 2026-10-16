using Datinate.App.Shared.Projects;
using Datinate.App.WinForms.View.Util;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;

namespace Datinate.App.WinForms.View.UI.ProjectPathsUpdate
{
    public partial class DatPathUpdateUI : UserControl 
    {
        public const string DEFAULT_NONE = "[Not Set]";

        DatGrouperProjectEntry? datHeadlineVO;

        public DatPathUpdateUI() 
        {
            InitializeComponent();
        }

        public void SetUI(DatGrouperProjectEntry datHeadlineVO) 
        {
            this.datHeadlineVO = datHeadlineVO;

            newPathLabel.Text = DEFAULT_NONE;
            oldPathLabel.Text = datHeadlineVO.DatFullpath;
            oldDatFilenameLabel.Text = Path.GetFileName(datHeadlineVO.DatFullpath);

            datTypeLabel.Text = datHeadlineVO.DatGroupEnum.ToString();

            if (!string.IsNullOrWhiteSpace(datHeadlineVO.InternalDescriptor)) 
            {
                datTypeLabel.Text += " > " + datHeadlineVO.InternalDescriptor;
            }

            if (datHeadlineVO.DatSubsetFilter != null) 
            {
                if (!string.IsNullOrWhiteSpace(datHeadlineVO.DatSubsetFilter.Path))
                    datTypeLabel.Text += " > " + datHeadlineVO.DatSubsetFilter.Path;
                
                if (!string.IsNullOrWhiteSpace(datHeadlineVO.DatSubsetFilter.Entry))
                    datTypeLabel.Text += " > " + datHeadlineVO.DatSubsetFilter.Entry;
            }
        }
        public DatUpdateVO? GetVO() 
        {
            if (datHeadlineVO == null) return null;

            return new DatUpdateVO(
                newPathLabel.Text
                , datHeadlineVO
            );
        }

        public string GetOldFullpath() => oldPathLabel.Text;
        
        public string GetNewFullpath() => newPathLabel.Text;

        void NewPathBtn_Click(object sender, EventArgs e) 
        {    
            string dir = Path.GetDirectoryName(oldPathLabel.Text) ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                dir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            OpenFileDialog d = new OpenFileDialog();

            d.Title = "Select Replacement DAT";
            d.InitialDirectory = dir;
            
            if (d.ShowDialog() == DialogResult.OK) 
                newPathLabel.Text = d.FileName;
        }


        private void quickFindBtn_LinkClicked(
            object sender,
            LinkLabelLinkClickedEventArgs e)
        {
            string? replacement =
                DatReplacementFinder.FindReplacement(oldPathLabel.Text);

            if (string.IsNullOrWhiteSpace(replacement))
            {
                FormsHelper.ShowWarningDialog("Could not find a similar file.");
                return;
            }

            newPathLabel.Text = replacement;
        }
    }
}
