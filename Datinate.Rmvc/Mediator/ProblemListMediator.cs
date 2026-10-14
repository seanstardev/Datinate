using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    public class ProblemListMediator : RMediator<IProblemListView>
    {
        public void SetUnreadableView(string[] problemItems) 
            => actor?.SetUnreadableView(problemItems);
        
        public void SetDuplicatesView(string[] problemItems) 
            => actor?.SetDuplicatesView(problemItems);
        
        public void ClearView() 
            => actor?.ClearView();
        
        protected override void Initialised()
        {

        }

        protected override void Disposing()
        {

        }
    }
}
