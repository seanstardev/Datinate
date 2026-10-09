using RMVC;
using System.Diagnostics;

namespace com.RADIO.Datinate.RMVC
{
    internal class SetProblemListCmd : RCommand
    {
        public enum UI { Duplicates, Unreadable }

        private readonly UI ui;
        private readonly string[] problemItems;

        public SetProblemListCmd(UI ui, string[] problemItems) 
        {
            this.ui = ui;
            this.problemItems = problemItems;
        }

        protected override void Run() 
        {
            var instance = Facade.Instance;

            Facade.Instance?.Shell?.SetProblemListFormVisible(true);

            ProblemListMediator? problemListMediator = instance?.ProblemListMediator;

            switch (ui) 
            {     
                case UI.Unreadable:
                    problemListMediator?.SetUnreadableView(problemItems);
                    break;

                case UI.Duplicates:
                    problemListMediator?.SetDuplicatesView(problemItems);
                    break;

                default:
                    Debug.WriteLine("SetProblemListCmd. UNKNOWN PROBLEM: " + ui);
                    break;
            }
        }
    }
}
