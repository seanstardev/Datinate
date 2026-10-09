namespace datinate.app {

    public partial class ProblemListUI : UserControl 
    {
        public ProblemListUI() 
        {
            InitializeComponent();
        }

        public void setView(string title, string description) 
        {
            Ui(() =>
            {
                problemNameLabel.Text = title;
                descriptionTextbox.Text = description;

            });
        }

        public void setList(string[] problemItems)
        {
            if (itemTextbox.InvokeRequired)
            {
                itemTextbox.Invoke(new Action(() => setList(problemItems)));
                return;
            }

            string content = "";
            for (int i = 0; i < problemItems.Length; i++)
            {
                content += $"{i + 1}: {problemItems[i]}\n";
            }

            itemTextbox.Text = content;
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
