using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class UpdateMediaCollectionMetaCmd : CommandBase<RVoid>
    {
        private readonly IGameFamily family;
        private readonly HashSet<string> checkedDescriptors;
        private readonly string familyNotesText;
        private readonly bool descriptorsChanged;

        public UpdateMediaCollectionMetaCmd(
            IGameFamily family, 
            HashSet<string> checkedDescriptors, 
            string familyNotesText,
            bool descriptorsChanged)
        {
            this.family = family;
            this.checkedDescriptors = checkedDescriptors;
            this.familyNotesText = familyNotesText;
            this.descriptorsChanged = descriptorsChanged;
        }

        protected override async Task<RVoid> Run()
        {
            if (base.RadioDatModel is { })
            {
                var cache = base.RadioDatModel.UpdateMediaCollectionMeta(family, checkedDescriptors, familyNotesText);
                await base.Execute(new SetDatGrouperViewMediaCache(cache));

                // TODO: DRY with meda assignment command.
                if (descriptorsChanged)
                {
                    if (base.RadioDatModel is { } &&
                        base.DatGrouperSessionModel is { } &&
                        base.DatGrouperSessionModel.IsInMediaAssignmentMode)
                    {
                        var scoring = base.RadioDatModel.GetScoring(family);
                        
                        if (scoring != null)
                            base.MediaAssignmentMediator?.SetScoring(scoring);
                    }
                }
            }

            return await Complete();
        }
    }
}
