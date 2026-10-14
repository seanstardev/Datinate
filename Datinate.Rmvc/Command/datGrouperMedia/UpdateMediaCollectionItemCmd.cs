using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class UpdateMediaCollectionItemCmd : CommandBase<RVoid>
    {
        private readonly RbMediaItemAssignmentUpdate assignment;

        public UpdateMediaCollectionItemCmd(RbMediaItemAssignmentUpdate assignment)
        {
            this.assignment = assignment;
        }

        protected override async Task<RVoid> Run()
        {
            var cache = base.RadioDatModel?.UpdateMediaCollectionItem(assignment);
            
            if (cache != null)
                base.DatGrouperMediator?.SetMediaCache(cache);

            // TODO: DRY with meta updates:
            if (base.RadioDatModel != null &&
                base.DatGrouperSessionModel != null &&
                base.DatGrouperSessionModel.IsInMediaAssignmentMode)
            {
                var scoring = base.RadioDatModel.GetScoring(assignment.Family);
                
                if (scoring != null) 
                    base.MediaAssignmentMediator?.SetScoring(scoring);
                
            }

            return await Complete();
        }
    }
}
