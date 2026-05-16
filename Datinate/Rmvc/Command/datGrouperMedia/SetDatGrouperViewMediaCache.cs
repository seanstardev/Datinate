using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetDatGrouperViewMediaCache : RCommand
    {
        private readonly IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache;

        public SetDatGrouperViewMediaCache(IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache)
        {
            this.mediaCache = mediaCache;
        }

        protected override void Run()
        {
            Facade.Instance?.DatGrouperMediator?.SetMediaCache(mediaCache);
        }
    }
}
