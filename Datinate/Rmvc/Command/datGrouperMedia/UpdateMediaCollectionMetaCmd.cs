using RadioLibCore.RadioDat;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class UpdateMediaCollectionMetaCmd : RCommand
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

        protected override void Run()
        {
            if (Facade.Instance?.RadioDatModel is { } radioDatModel)
            {
                var cache = Facade.Instance.RadioDatModel.UpdateMediaCollectionMeta(family, checkedDescriptors, familyNotesText);
                base.ExecuteCommand(new SetDatGrouperViewMediaCache(cache));

                // TODO: DRY with meda assignment command.
                if (descriptorsChanged)
                {
                    if (Facade.Instance?.RadioDatModel is { } radioModel &&
                        Facade.Instance?.DatGrouperSessionModel is { } sessionModel &&
                        sessionModel.IsInMediaAssignmentMode)
                    {
                        var scoring = radioModel.GetScoring(family);
                        if (scoring != null)
                        {
                            Facade.Instance?.MediaAssignmentMediator?.SetScoring(scoring);
                        }
                    }
                }
            }
        }
    }
}
