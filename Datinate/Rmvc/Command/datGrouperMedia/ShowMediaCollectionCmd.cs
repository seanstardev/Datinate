using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using RadioLibCore.RadioDat;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ShowMediaCollectionCmd : RCommand
    {
        private readonly DatGrouperEntryDTO datGrouperEntryDTO;

        public ShowMediaCollectionCmd(DatGrouperEntryDTO datGrouperEntryDTO)
        {
            this.datGrouperEntryDTO = datGrouperEntryDTO;
        }

        protected override void Run()
        {
            var sessionModel = Facade.Instance?.DatGrouperSessionModel;

            if (sessionModel == null)
                return;

            if (!sessionModel.IsInMediaMode)
                return;

            Facade.Instance?.MediaWebMediator?.ClearView();

            var filter = DatinateHelper.GetGameEntityName(datGrouperEntryDTO.Entity);

            if (string.IsNullOrWhiteSpace(filter))
                return;

            if (sessionModel.IsInMediaReadOnlyMode)
            {
                Facade.Instance?.MediaMediator?.ShowViewPreview(filter!);

                // TODO: ?:
                Facade.Instance?.DatGrouperMediator?.SetMediaMode(sessionModel.CurrentLayout);
            }

            else if (datGrouperEntryDTO.Entity is IGameFamily family)
            {
                if (Facade.Instance?.RadioDatModel is { } radioDatModel)
                {
                    var collection = radioDatModel.GetMediaCollection(family);

                    if (collection != null)
                    {
                        Facade.Instance?.MediaMediator?.ShowViewAssign(filter!, collection);
                        Facade.Instance?.MediaAssignmentMediator?.SetView(collection);

                        var scoring = radioDatModel.GetScoring(family);
                        if (scoring != null)
                            Facade.Instance?.MediaAssignmentMediator?.SetScoring(scoring);

                        Facade.Instance?.DatGrouperMediator?.SetMediaMode(sessionModel.CurrentLayout);
                    }
                }
            }
        }
    }
}
