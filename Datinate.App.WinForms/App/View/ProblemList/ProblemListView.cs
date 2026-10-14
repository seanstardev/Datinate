using Datinate.Rmvc;
using Datinate.Shared.Rmvc;

namespace Datinate.App.WinForms.View
{
    public partial class ProblemListView : UserControl, IProblemListView 
    {
        private readonly Datinate.App.WinForms.View.Util.UiDispatcher uiDispatcher;

        public ProblemListView() 
        {
            uiDispatcher = new Datinate.App.WinForms.View.Util.UiDispatcher(this);
            InitializeComponent();

            unreadableUI.setView(
                "Unrecognised DAT files found"
                , "The following list of files could not be read. They will be ignored by Datinate. Please get in touch if you expect these files to be supported."

            );

            duplicatesUI.setView(
                "Matching DAT files found"
                , "You may want to remove the below duplicates. They have matching checksums."
            );

            ClearView();
            Facade.RegisterActor<IProblemListView>(this);
        }
        protected void HandleDisposing()
        {
            Facade.UnregisterActor<IProblemListView>(this);
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
            uiDispatcher.Run(action);
        }
    }
}