using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using Datinate.Shared;

namespace com.RADIO.Datinate.App.View.projects.datPathsUpdate
{
    public partial class DatPathsUpdateView : UserControl, IDatPathsUpdateView 
    {
        public event Action<DatGrouperProjectDTO>? SaveClickEvt;

        DatGrouperProjectDTO? projectVO;

        public DatPathsUpdateView() 
        {
            InitializeComponent();
            Facade.RegisterActor(this);
        }

        public void SetView(DatGrouperProjectDTO projectVO)
        {
            Ui(() => 
            { 

                ClearView();

                this.projectVO = projectVO;

                List<DatGrouperProjectEntry> list = new List<DatGrouperProjectEntry>();
                DatGrouperProjectEntry[] paths = DatGrouperProjectDTO.GetAllProjectEntries(projectVO);
                DatGrouperProjectEntry vo;

                for (int i = 0; i < paths.Length; i++) 
                {
                    vo = paths[i];
                    if (!File.Exists(vo.DatFullpath)) 
                    {
                        list.Add(vo);
                    }
                }
                
                DatPathUpdateUI ui;
                
                for (int i = 0; i < list.Count; i++) 
                {
                    ui = new DatPathUpdateUI();
                    ui.SetUI(list[i]);
                    pathsContainer.Controls.Add(ui);
                }
             });
        }

        private DatGrouperProjectDTO? GetProjectVO() 
        {
            return projectVO;
        }

        private DatUpdateVO[] GetUpdatVOs() 
        {
            List<DatUpdateVO> list = new List<DatUpdateVO>();
            DatPathUpdateUI? ui;
            for (int i = 0; i < pathsContainer.Controls.Count; i++) 
            {
                ui = pathsContainer.Controls[i] as DatPathUpdateUI;
                if (ui == null)
                    continue;

                list.Add(ui.GetVO());
            }
            return list.ToArray();
        }

        private void ClearView() 
        {
            pathsContainer.Controls.Clear();
        }
        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        private void ShowError() 
        {
            MessageBox.Show(
                    "Ensure all new DAT fullpaths are set, and that they are unique."
                    , "There was a Problem"
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Warning
                );
        }
        private void SaveBtn_Click(object sender, EventArgs e) 
        {

            if (SaveClickEvt == null) return;

            DatUpdateVO[] vos = GetUpdatVOs();
            for (int i = 0; i < vos.Length; i++) 
            {
                if (string.IsNullOrWhiteSpace(vos[i].NewFullpath) || !File.Exists(vos[i].NewFullpath)) 
                {
                    ShowError();
                    return;
                }
            }

            DatGrouperProjectEntry[] headlines = DatGrouperProjectDTO.GetAllProjectEntries(projectVO);
            
            for (int i = 0; i < headlines.Length; i++) 
            {
                var headline = headlines[i];

                for (int j = 0; j < vos.Length; j++) 
                {
                    if (IsMatch(vos[j], headline)) 
                    {
                        headline.DatFullpath = vos[j].NewFullpath;
                        break;
                    }
                }
            }

            var project = GetProjectVO();
            if (project != null)
                SaveClickEvt.Invoke(project);
        }

        private bool IsMatch(DatUpdateVO updateVO, DatGrouperProjectEntry headlineVO) 
        {
            if (updateVO.DatHeadlineVO.DatFullpath != headlineVO.DatFullpath)
                return false;

            if (updateVO.DatHeadlineVO.DatGroupEnum != headlineVO.DatGroupEnum)
                return false;

            if (!DatSubsetFilter.Equals(updateVO.DatHeadlineVO.DatSubsetFilter, headlineVO.DatSubsetFilter))
                return false;

            return true;
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
