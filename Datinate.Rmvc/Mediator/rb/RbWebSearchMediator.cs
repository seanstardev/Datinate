using Datinate.Rmvc.Command;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    public class RbWebSearchMediator : RMediator<IRbWebSearchView>
    {
        public void SetSearchTerms(string? gameName, string? systemName) =>
            actor?.SetSearchTerms(gameName, systemName);
        
        public void ClearView() =>
            actor?.ClearView();

        public void SetSearchEngine(WEB_SOURCE_ENUM engine, bool invokeSearchNow)
            => actor?.SetSearchEngine(engine, invokeSearchNow);
        
        public void SearchCurrent()
            => actor?.SearchCurrent();
        
        private void OnLoadUrl(string url)
            => base.Execute(new LoadWebUrlCmd(url));

        protected override void Initialised()
        {
            if (actor == null) return;

            actor.LoadUrlEvt += OnLoadUrl;
        }

        protected override void Disposing()
        {
            if (actor == null) return;

            actor.LoadUrlEvt -= OnLoadUrl;
        }
    }
}
