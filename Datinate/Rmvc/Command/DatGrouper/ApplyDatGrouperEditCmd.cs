using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ApplyDatGrouperEditCmd : RCommand
    {
        private readonly IDatGrouperDelta delta;
        private readonly bool applyMediaReferenceUpdates;

        public ApplyDatGrouperEditCmd(IDatGrouperDelta delta, bool applyMediaReferenceUpdates)
        {
            this.delta = delta;
            this.applyMediaReferenceUpdates = applyMediaReferenceUpdates;
        }

        protected override void Run()
        {
            if (delta != null)
            {
                Facade.Instance?.DatGrouperMediator?.UpdateAutoFamilies(
                    delta.DeltaNatureEnum,
                    delta.AutoFamiliesToAdd,
                    delta.AutoFamiliesToRemove,
                    delta.AllCuratedAutoParts,
                    delta.AvailableUndos,
                    delta.AvailableRedos,
                    delta.AutoAffectedEntities);

                Facade.Instance?.DatGrouperMediator?.UpdateCuratedFamilies(
                    delta.DeltaNatureEnum,
                    delta.CuratedFamiliesToAdd,
                    delta.CuratedFamiliesToRemove,
                    delta.AvailableUndos,
                    delta.AvailableRedos,
                    delta.CuratedAffectedEntities);

                if (applyMediaReferenceUpdates)
                {
                    var media = Facade.Instance?.RadioDatModel?.UpdateMediaReferences(delta.ReplacementReferences);

                    if (media != null)
                        Facade.Instance?.DatGrouperMediator?.SetMediaCache(media);
                }

                if (Facade.Instance?.DatGrouperModel is { } grouperModel &&
                    Facade.Instance?.DatGrouperSessionModel is { } sessionModel)
                {
                    var partsCurated = DatinateHelper.GetTotalParts(grouperModel.CuratedFamilies);

                    sessionModel.PartsCurated = partsCurated;

                    Facade.Instance?.DatGrouperControlsMediator?.SetCompletionStats(
                        sessionModel.PartsCurated, sessionModel.PartsTotal);
                }
            }
        }
    }
}
