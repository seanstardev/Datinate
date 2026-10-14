using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetWebSearchTermsCmd : CommandBase<RVoid>
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

        protected override async Task<RVoid> Run()
        {
            if (base.RbWebSearchMediator == null)
                return await Complete();

            base.RbWebSearchMediator.SetSearchTerms(
                gameName, systemName);

            if (searchNow)
                base.RbWebSearchMediator.SearchCurrent();

            return await Complete();
        }
    }
}
