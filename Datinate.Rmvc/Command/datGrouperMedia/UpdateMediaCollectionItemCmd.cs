using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class UpdateMediaCollectionItemCmd : RCommand
    {
        private readonly RbMediaItemAssignmentUpdate assignment;

        public UpdateMediaCollectionItemCmd(RbMediaItemAssignmentUpdate assignment)
        {
            this.assignment = assignment;
        }

        protected override void Run()
        {
            var cache = Facade.Instance?.RadioDatModel?.UpdateMediaCollectionItem(assignment);
            
            if (cache != null)
                Facade.Instance?.DatGrouperMediator?.SetMediaCache(cache);

            // TODO: DRY with meta updates:
            if (Facade.Instance?.RadioDatModel is { } radioModel && 
                Facade.Instance?.DatGrouperSessionModel is { } sessionModel &&
                sessionModel.IsInMediaAssignmentMode)
            {
                var scoring = radioModel.GetScoring(assignment.Family);
                if (scoring != null) 
                {
                    Facade.Instance?.MediaAssignmentMediator?.SetScoring(scoring);
                }
            }
        }
    }
}
