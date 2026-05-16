using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class RbWebSearchMediator : RMediator
    {
        private IRbWebSearchView? view => (IRbWebSearchView?)base.viewBase;
        public RbWebSearchMediator(Type view) : base(view)
        {
        }
        public void SetSearchTerms(string? gameName, string? systemName) =>
            view?.SetSearchTerms(gameName, systemName);
        
        public void ClearView() =>
            view?.ClearView();

        protected override void Disposing()
        {
            if (view == null) return;

            view.LoadUrlEvt -= OnLoadUrl;
        }

        private void OnLoadUrl(string url)
        {
            base.ExecuteCommand(new LoadWebUrlCmd(url));
        }

        protected override void Initialsed()
        {
            if (view == null) return;

            view.LoadUrlEvt += OnLoadUrl;
        }

        internal void SearchCurrent()
        {
            view?.SearchCurrent();
        }
    }
}
