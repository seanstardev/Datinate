using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class HighlightProjectDatsCmd : RCommand
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

        protected override void Run() 
        {
            Facade.Instance?.DatSummaryMediator?.HighlightDats(
                includeDatHeadlineVOs
                , ignoreDatHeadlineVOs
            );
        }

        // TODO: Remove this. Not sure highlight project is useful at all.
        public static void Execute() 
        {
            Facade.Instance?.DatSummaryMediator?.HighlightDats(
                new DatGrouperProjectEntry[] { }
                , new DatGrouperProjectEntry[] { });
        }
    }
}
