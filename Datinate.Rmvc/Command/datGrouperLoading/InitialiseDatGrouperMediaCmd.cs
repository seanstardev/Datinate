using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class InitialiseDatGrouperMediaCmd : CommandBase<RVoid>
    {
        private readonly IReadOnlyList<ILookupSet> mediaAndResources;

        public InitialiseDatGrouperMediaCmd(IReadOnlyList<ILookupSet> mediaAndResources)
        {
            this.mediaAndResources = mediaAndResources;
        }

        protected override async Task<RVoid> Run()
        {
            if (base.DatGrouperSessionModel == null ||
                base.DatGrouperSessionModel.ContentPathsResolved == false ||
                base.DatGrouperSessionModel.MediaInitialisd)
            {
                return await Complete();
            }

            var payload = mediaAndResources.Where(l => !l.RadioSource.Hide).ToArray();

            base.MediaMediator?.InitialiseView(payload);
            base.DatGrouperSessionModel.MediaInitialisd = true;

            if (payload.Length > 0)
                base.MainWebMediator?.SetMediaIsAvailable();

            return await Complete();
        }
    }
}
