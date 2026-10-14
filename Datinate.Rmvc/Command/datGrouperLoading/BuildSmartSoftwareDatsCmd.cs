using Datinate.Rmvc.Dto;
using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.DatGrouper;

namespace Datinate.Rmvc.Command
{
    internal class BuildSmartSoftwareDatsCmd : CommandBase<IReadOnlyList<DatAdvanced>?>
    {
        private readonly DatGrouperProjectDTO projectVO;

        public BuildSmartSoftwareDatsCmd(DatGrouperProjectDTO projectVO)
        {
            this.projectVO = projectVO;
        }

        protected override async Task<IReadOnlyList<DatAdvanced>?> Run()
        {
            if (base.RadioDatModel == null ||
                base.GlobalSettingsProxy == null ||
                base.DatDetailsProxy == null ||
                base.ExpressionsProxy == null)
                return null;

            var priority = 0;

            var advs = new List<DatAdvanced>();

            foreach (var datHeadline in projectVO.SoftwareEntries)
            {
                DatVO? dat = base.DatDetailsProxy.GetDat(datHeadline.DatFullpath);

                if (dat == null) throw new Exception();

                dat = base.DatDetailsProxy.ApplyDatSubfilters(dat, datHeadline.DatSubsetFilter);

                if (dat == null) continue;

                DatFilter[] filters = datHeadline.HasExpressionFilterDat ?
                    base.ExpressionsProxy.FetchExpressions(datHeadline.ExpressionsXmlFullpath)
                    : Array.Empty<DatFilter>();

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

            base.RadioDatModel.AddDats(datsDic);

            return await Complete(advs);
        }
    }
}
