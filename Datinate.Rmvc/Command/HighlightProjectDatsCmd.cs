using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class HighlightProjectDatsCmd : CommandBase<RVoid>
    {
        private readonly DatGrouperProjectEntry[] includeDatHeadlineVOs;
        private readonly DatGrouperProjectEntry[] ignoreDatHeadlineVOs;

        public HighlightProjectDatsCmd(
            DatGrouperProjectEntry[] includeDatHeadlineVOs
            , DatGrouperProjectEntry[] ignoreDatHeadlineVOs) 
        {
            this.includeDatHeadlineVOs = includeDatHeadlineVOs;
            this.ignoreDatHeadlineVOs = ignoreDatHeadlineVOs;
        }

        protected override async Task<RVoid> Run() 
        {
            base.DatSummaryMediator?.HighlightDats(
                includeDatHeadlineVOs
                , ignoreDatHeadlineVOs
            );

            return await Complete();
        }

        // TODO: Remove this. Not sure highlight project is useful at all.
        //public static void Execute() 
        //{
        //    Facade.Instance?.DatSummaryMediator?.HighlightDats(
        //        new DatGrouperProjectEntry[] { }
        //        , new DatGrouperProjectEntry[] { });
        //}
    }
}
