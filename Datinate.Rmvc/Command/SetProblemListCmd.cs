using Datinate.Rmvc.Mediator;
using RMVC;
using System.Diagnostics;

namespace Datinate.Rmvc.Command
{
    internal class SetProblemListCmd : CommandBase<RVoid>
    {
        public enum UI { Duplicates, Unreadable }

        private readonly UI ui;
        private readonly string[] problemItems;

        public SetProblemListCmd(UI ui, string[] problemItems) 
        {
            this.ui = ui;
            this.problemItems = problemItems;
        }

        protected override async Task<RVoid> Run() 
        {
            base.Shell?.SetProblemListFormVisible(true);

            switch (ui) 
            {     
                case UI.Unreadable:
                    base.ProblemListMediator?.SetUnreadableView(problemItems);
                    break;

                case UI.Duplicates:
                    base.ProblemListMediator?.SetDuplicatesView(problemItems);
                    break;

                default:
                    Debug.WriteLine("SetProblemListCmd. UNKNOWN PROBLEM: " + ui);
                    break;
            }

            return await Complete();
        }
    }
}
