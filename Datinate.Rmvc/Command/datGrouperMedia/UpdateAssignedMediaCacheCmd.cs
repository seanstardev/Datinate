using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class UpdateAssignedMediaCacheCmd : CommandBase<RVoid>
    {
        private readonly IReadOnlyDictionary<string, HashSet<string>> assignedEntriesCache;

        public UpdateAssignedMediaCacheCmd(
            IReadOnlyDictionary<string, HashSet<string>> assignedEntriesCache)
        {
            this.assignedEntriesCache = assignedEntriesCache;
        }

        protected override async Task<RVoid> Run()
        {
            base.MediaMediator?.SetAssignedEntriesCache(assignedEntriesCache);

            return await Complete();
        }
    }
}
