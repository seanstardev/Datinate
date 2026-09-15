using datinate.app;
using Datinate.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

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

        public void SetSearchEngine()
        {

        }
        public void SetSearchEngine(WEB_SOURCE_ENUM engine, bool invokeSearchNow)
        {
            view?.SetSearchEngine(engine, invokeSearchNow);
        }
        public void SearchCurrent()
        {
            view?.SearchCurrent();
        }
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
    }
}
