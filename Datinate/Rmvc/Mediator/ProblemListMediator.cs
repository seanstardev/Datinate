using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ProblemListMediator : RMediator 
    {
        private IProblemListView? view => (IProblemListView?)base.viewBase;
        public ProblemListMediator(Type actor) : base(actor)
        {
        }

        public void SetUnreadableView(string[] problemItems) 
        {
            view?.SetUnreadableView(problemItems);
        }

        public void SetDuplicatesView(string[] problemItems) 
        {
            view?.SetDuplicatesView(problemItems);
        }

        public void ClearView() 
        {
            view?.ClearView();
        }

        protected override void Initialsed()
        {

        }

        protected override void Disposing()
        {

        }
    }
}
