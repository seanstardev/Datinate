using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC;
using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class BuildSmartSoftwareDatsCmd : RCommandAsync
    {
        public IReadOnlyList<DatAdvanced>? DatAdvancedCollection;
        private readonly DatGrouperProjectDTO projectVO;

        public BuildSmartSoftwareDatsCmd(DatGrouperProjectDTO projectVO)
        {
            this.projectVO = projectVO;
        }

        protected override Task RunAsync()
        {
            var radioDatModel = Facade.Instance?.RadioDatModel;
            var globalSettings = Facade.Instance?.GlobalSettingsProxy;
            var datDetailsProxy = Facade.Instance?.DatDetailsProxy;
            var expressionsProxy = Facade.Instance?.ExpressionsProxy;

            if (radioDatModel == null || 
                globalSettings == null ||
                datDetailsProxy == null ||
                expressionsProxy == null)
                return Task.CompletedTask;

            var priority = 0;

            var advs = new List<DatAdvanced>();

            foreach (var datHeadline in projectVO.SoftwareEntries)
            {
                DatVO? dat = datDetailsProxy.GetDat(datHeadline.DatFullpath);

                if (dat == null) throw new Exception();

                dat = datDetailsProxy.ApplyDatSubfilters(dat, datHeadline.DatSubsetFilter);

                if (dat == null) continue;

                DatFilter[] filters = datHeadline.HasExpressionFilterDat ? 
                    expressionsProxy.FetchExpressions(datHeadline.ExpressionsXmlFullpath)
                    : [];

                advs.Add(new DatAdvanced(
                    dat,
                    datHeadline.ID,
                    datHeadline.DatGroupEnum,
                    datHeadline.FriendlyName.Trim(),
                    filters,
                    datHeadline.DatSubsetFilter));

                priority++;
            }

            Dictionary<string, DatVO> datsDic = new Dictionary<string, DatVO>();
            
            foreach (var adv in advs)
            {
                datsDic.Add(adv.Key, adv.Dat);
            }
            
            radioDatModel.AddDats(datsDic);

            DatAdvancedCollection = advs;
            return Task.CompletedTask;
        }
    }
}
