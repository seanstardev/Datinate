using Datinate.Shared;

namespace com.RADIO.Datinate.App.View.problemList
{
    public partial class ProblemListView : UserControl, IProblemListView 
    {
        public ProblemListView() 
        {
            InitializeComponent();

            unreadableUI.setView(
                "Unrecognised DAT files found"
                , "The following list of files could not be read. They will be ignored by Datinate. Please get in touch if you expect these files to be supported."

            );

            duplicatesUI.setView(
                "Matching DAT files found"
                , "You may want to remove the below duplicates.They have matching checksums."
            );

            ClearView();
            Facade.RegisterActor(this);
        }
        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        public void SetUnreadableView(string[] problemItems) 
        {
            Ui(() =>
            {
                unreadableUI.setList(problemItems);
                tabControl.SelectedIndex = 0;
            });
        }
        public void SetDuplicatesView(string[] problemItems) 
        {
            Ui(() =>
            {
                duplicatesUI.setList(problemItems);
                tabControl.SelectedIndex = 1;
            });
        }


        public void ShowAndBringToFront() 
        {
            Ui(() =>
            {
                BringToFront();
                Show();
            });
        }

        public void ClearView() {
            Ui(() =>
            {
                duplicatesUI.setList(new string[] { });
                unreadableUI.setList(new string[] { });
                this.Hide();
                tabControl.SelectedIndex = 0;
            });
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