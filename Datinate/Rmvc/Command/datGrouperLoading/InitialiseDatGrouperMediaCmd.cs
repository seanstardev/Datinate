using Datinate.Shared.Rb;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class InitialiseDatGrouperMediaCmd : RCommand
    {
        private readonly IReadOnlyList<ILookupSet> mediaAndResources;

        public InitialiseDatGrouperMediaCmd(IReadOnlyList<ILookupSet> mediaAndResources)
        {
            this.mediaAndResources = mediaAndResources;
        }

        protected override void Run()
        {
            var sessionModel = Facade.Instance?.DatGrouperSessionModel;

            if (sessionModel == null ||
                sessionModel.ContentPathsResolved == false ||
                sessionModel.MediaInitialisd)
            {
                return;
            }

            var payload = mediaAndResources.Where(l => !l.RadioSource.Hide).ToArray();

            Facade.Instance?.MediaMediator?.InitialiseView(payload);
                sessionModel.MediaInitialisd = true;

            if (payload.Length > 0)
                Facade.Instance?.MainWebMediator?.SetMediaIsAvailable();
        }
    }
}
