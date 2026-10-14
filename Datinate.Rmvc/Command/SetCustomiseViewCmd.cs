using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.DatGrouper;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal sealed class SetCustomiseViewCmd : CommandBase<RVoid>
    {
        private readonly DatGrouperProjectEntry? datHeadlineVO;
        private readonly DatVO? datVO;

        public SetCustomiseViewCmd(DatGrouperProjectEntry datHeadlineVO)
        {
            this.datHeadlineVO = datHeadlineVO;
        }

        public SetCustomiseViewCmd(DatVO datVO)
        {
            this.datVO = datVO;
        }

        protected override async Task<RVoid> Run()
        {
            if (datHeadlineVO != null)
                await LoadFromHeadline(datHeadlineVO);
            else if (datVO != null)
                await LoadFromDat(datVO);

            return await Complete();
        }

        private async Task LoadFromHeadline(DatGrouperProjectEntry datHeadlineVO)
        {
            var datVO = base.DatDetailsProxy?.GetDat(datHeadlineVO.DatFullpath);
            if (datVO == null)
                return;

            base.ActiveDatsModel!.CustomiseDat = datVO;

            var flags = GetFlags(datVO);
            var cats = GetCategories(datVO);

            var expressions =
                datHeadlineVO.ExpressionsXmlFullpath != null
                    ? base.ExpressionsProxy?.FetchExpressions(datHeadlineVO.ExpressionsXmlFullpath) ?? Array.Empty<DatFilter>()
                    : Array.Empty<DatFilter>();

            base.CustomListMediator?.SetView(datVO, flags, cats, expressions);
            base.Shell?.SetCustomFormVisible(true);

            await base.Execute(new SetMainControlsBtnEnabledCmd(
                MAIN_CONTROL_ENUM.DatCustomiser, true));
        }

        private async Task LoadFromDat(DatVO datVO)
        {
            if (base.ActiveDatsModel is { })
                base.ActiveDatsModel.CustomiseDat = datVO;

            var flags = GetFlags(datVO);
            var cats = GetCategories(datVO);

            base.CustomListMediator?.SetView(datVO, flags, cats);
            base.Shell?.SetCustomFormVisible(true);

            await base.Execute(new SetMainControlsBtnEnabledCmd(
                MAIN_CONTROL_ENUM.DatCustomiser, true));
        }

        private static Flag[] GetCategories(DatVO datVO) =>
            DatFilterHelper.GetCategorySets(datVO); 
        
        private static Flag[] GetFlags(DatVO datVO) =>
            DatFilterHelper.GetFlagSets(datVO.Entries.ToArray());
    }
}