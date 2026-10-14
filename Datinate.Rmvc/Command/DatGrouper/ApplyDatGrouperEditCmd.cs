using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ApplyDatGrouperEditCmd : CommandBase<RVoid>
    {
        private readonly IDatGrouperDelta delta;
        private readonly bool applyMediaReferenceUpdates;

        public ApplyDatGrouperEditCmd(IDatGrouperDelta delta, bool applyMediaReferenceUpdates)
        {
            this.delta = delta;
            this.applyMediaReferenceUpdates = applyMediaReferenceUpdates;
        }

        protected override async Task<RVoid> Run()
        {
            if (delta != null)
            {
                base.DatGrouperMediator?.UpdateAutoFamilies(
                    delta.DeltaNatureEnum,
                    delta.AutoFamiliesToAdd,
                    delta.AutoFamiliesToRemove,
                    delta.AllCuratedAutoParts,
                    delta.AvailableUndos,
                    delta.AvailableRedos,
                    delta.AutoAffectedEntities);

                base.DatGrouperMediator?.UpdateCuratedFamilies(
                    delta.DeltaNatureEnum,
                    delta.CuratedFamiliesToAdd,
                    delta.CuratedFamiliesToRemove,
                    delta.AvailableUndos,
                    delta.AvailableRedos,
                    delta.CuratedAffectedEntities);

                if (applyMediaReferenceUpdates)
                {
                    var media = base.RadioDatModel?.UpdateMediaReferences(
                        delta.ReplacementReferences);

                    if (media != null)
                        base.DatGrouperMediator?.SetMediaCache(media);
                }

                if (base.DatGrouperModel is { } grouperModel &&
                    base.DatGrouperSessionModel is { } sessionModel)
                {
                    var partsCurated = DatinateFamilyHelper.GetTotalParts(
                        grouperModel.CuratedFamilies);

                    sessionModel.PartsCurated = partsCurated;

                    base.DatGrouperControlsMediator?.SetCompletionStats(
                        sessionModel.PartsCurated, sessionModel.PartsTotal);
                }
            }

            return await Complete();
        }
    }
}
