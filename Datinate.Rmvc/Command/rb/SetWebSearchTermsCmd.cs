using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetWebSearchTermsCmd : RCommand
    {
        private readonly string? gameName;
        private readonly string? systemName;
        private readonly bool searchNow;

        public SetWebSearchTermsCmd(string? gameName, string? systemName, bool searchNow = false)
        {
            this.gameName = gameName;
            this.systemName = systemName;
            this.searchNow = searchNow;
        }

        protected override void Run()
        {
            var webSearch = Facade.Instance?.RbWebSearchMediator;
            
            if (webSearch == null)
                return;

            webSearch.SetSearchTerms(gameName, systemName);

            if (searchNow)
                webSearch.SearchCurrent();
        }
    }
}
