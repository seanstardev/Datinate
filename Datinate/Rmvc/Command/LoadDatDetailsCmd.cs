using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class LoadDatDetailsCmd : RCommand 
    {
        private readonly string datFullpath;

        public LoadDatDetailsCmd(string datFullpath) 
        {
            this.datFullpath = datFullpath;
        }

        protected override void Run() 
        {

            var summary = Facade.Instance?.ActiveDatsModel?.GetDatSummary(datFullpath);
            if (summary == null) 
            {
                base.ExecuteCommand(new ShowMessageCmd(
                    "The selected DAT cannot be found. Please rescan your DAT directories."));

                return;
            }

            var datVO = Facade.Instance?.DatDetailsProxy?.GetDat(datFullpath, summary.DatHeader.DatTypeEnum);
            
            if (datVO == null)
            {
                base.ExecuteCommand(new ShowMessageCmd(
                    "The selected DAT's details cannot be parsed."));
            }
            else
            {
                if (Facade.Instance?.UnitDisplayModel != null)
                {
                    Facade.Instance?.DatDetailsMediator?.SetView(
                        datVO
                        , Facade.Instance.UnitDisplayModel.GetUnit()
                        , Facade.Instance.UnitDisplayModel.GetShowUnitInCells());
                }

                base.ExecuteCommand(new SetActiveDatModel(datVO, SetActiveDatModel.LOCAL_DAT_MODEL_ENUM.Detailed));
            }
        }
    }
}
