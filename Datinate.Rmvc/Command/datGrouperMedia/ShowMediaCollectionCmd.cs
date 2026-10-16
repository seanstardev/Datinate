using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ShowMediaCollectionCmd : CommandBase<RVoid>
    {
        private readonly DatGrouperEntryDTO datGrouperEntryDTO;

        public ShowMediaCollectionCmd(
            DatGrouperEntryDTO datGrouperEntryDTO)
        {
            this.datGrouperEntryDTO = datGrouperEntryDTO;
        }

        protected override async Task<RVoid> Run()
        {

            if (base.DatGrouperSessionModel == null)
                return await Complete();

            if (!base.DatGrouperSessionModel.IsInMediaMode)
                return await Complete();

            base.MediaWebMediator?.ClearView();

            var filters = DatinateFamilyHelper.GetAllNames(datGrouperEntryDTO.Entity);

            if (filters.Any() == false || string.IsNullOrWhiteSpace(filters.First()))
                return await Complete();


            if (base.DatGrouperSessionModel.IsInMediaReadOnlyMode)
            {
                base.MediaMediator?.ShowViewPreview(filters!);

                // TODO: ?:
                base.DatGrouperMediator?.SetMediaMode(DatGrouperSessionModel.CurrentLayout);
            }

            else if (datGrouperEntryDTO.Entity is IGameFamily family)
            {
                if (base.RadioDatModel != null)
                {
                    var collection = base.RadioDatModel.GetMediaCollection(family);

                    if (collection != null)
                    {
                        var prompts = DatinateFamilyHelper.CreateSearchPrompts(family);

                        base.MediaMediator?.ShowViewAssign(filters, collection, prompts);
                        base.MediaAssignmentMediator?.SetView(collection);

                        var scoring = base.RadioDatModel.GetScoring(family);
                        
                        if (scoring != null)
                        {
                            var summary = new MediaScoringSummaryDTO(scoring);
                            base.MediaAssignmentMediator?.SetScoring(scoring, summary);
                        }

                        // TODO: ?:
                        base.DatGrouperMediator?.SetMediaMode(DatGrouperSessionModel.CurrentLayout);
                    }
                }
            }

            return await Complete();
        }
    }
}
