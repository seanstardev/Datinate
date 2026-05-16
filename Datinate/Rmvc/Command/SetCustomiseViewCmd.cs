using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal sealed class SetCustomiseViewCmd : RCommand
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

        protected override void Run()
        {
            if (datHeadlineVO != null)
                LoadFromHeadline(datHeadlineVO);
            else if (datVO != null)
                LoadFromDat(datVO);
        }

        private void LoadFromHeadline(DatGrouperProjectEntry datHeadlineVO)
        {
            var facade = Facade.Instance;
            if (facade == null)
                return;

            var datVO = facade.DatDetailsProxy?.GetDat(datHeadlineVO.DatFullpath);
            if (datVO == null)
                return;

            facade.ActiveDatsModel!.CustomiseDat = datVO;

            var flags = GetFlags(datVO);
            var cats = GetCategories(datVO);

            var expressions =
                datHeadlineVO.ExpressionsXmlFullpath != null
                    ? facade.ExpressionsProxy?.FetchExpressions(datHeadlineVO.ExpressionsXmlFullpath) ?? Array.Empty<DatFilter>()
                    : Array.Empty<DatFilter>();

            facade.CustomListMediator?.SetView(datVO, flags, cats, expressions);
            facade.Shell?.SetCustomFormVisible(true);

            base.ExecuteCommand(new SetMainControlsBtnEnabledCmd(MAIN_CONTROL_ENUM.DatCustomiser, true));
        }

        private void LoadFromDat(DatVO datVO)
        {
            var facade = Facade.Instance;
            if (facade == null)
                return;

            facade.ActiveDatsModel!.CustomiseDat = datVO;

            var flags = GetFlags(datVO);
            var cats = GetCategories(datVO);

            facade.CustomListMediator?.SetView(datVO, flags, cats);
            facade.Shell?.SetCustomFormVisible(true);

            base.ExecuteCommand(new SetMainControlsBtnEnabledCmd(MAIN_CONTROL_ENUM.DatCustomiser, true));
        }
        private static Flag[] GetCategories(DatVO datVO) =>
            DatFilterHelper.GetCategorySets(datVO); 
        private static Flag[] GetFlags(DatVO datVO) =>
            DatFilterHelper.GetFlagSets(datVO.Entries.ToArray());
    }
}
