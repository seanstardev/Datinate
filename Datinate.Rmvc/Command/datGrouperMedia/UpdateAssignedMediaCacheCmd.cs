using RMVC;

namespace Datinate.Rmvc.Command
{
    public class UpdateAssignedMediaCacheCmd : RCommand
    {
        private readonly IReadOnlyDictionary<string, HashSet<string>> assignedEntriesCache;

        public UpdateAssignedMediaCacheCmd(
            IReadOnlyDictionary<string, HashSet<string>> assignedEntriesCache)
        {
            this.assignedEntriesCache = assignedEntriesCache;
        }

        protected override void Run()
        {
            Facade.Instance?.MediaMediator?.SetAssignedEntriesCache(assignedEntriesCache);
        }
    }
}
