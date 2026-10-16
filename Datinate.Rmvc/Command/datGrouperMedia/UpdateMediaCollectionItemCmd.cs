using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class UpdateMediaCollectionItemCmd : CommandBase<RVoid>
    {
        private readonly MediaItemAssignmentUpdate assignment;

        public UpdateMediaCollectionItemCmd(MediaItemAssignmentUpdate assignment)
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
                {
                    var summary = new MediaScoringSummaryDTO(scoring);
                    base.MediaAssignmentMediator?.SetScoring(scoring, summary);
                }
                
            }

            return await Complete();
        }
    }
}
