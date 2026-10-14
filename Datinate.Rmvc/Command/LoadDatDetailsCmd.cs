using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class LoadDatDetailsCmd : CommandBase<RVoid>
    {
        private readonly string datFullpath;

        public LoadDatDetailsCmd(string datFullpath) 
        {
            this.datFullpath = datFullpath;
        }

        protected override async Task<RVoid> Run() 
        {

            var summary = base.ActiveDatsModel?.GetDatSummary(datFullpath);
            if (summary == null) 
            {
                await base.Execute(new ShowMessageCmd(
                    "The selected DAT cannot be found. Please rescan your DAT directories."));

                return await Complete();
            }

            var datVO = base.DatDetailsProxy?.GetDat(datFullpath, summary.DatHeader.DatTypeEnum);
            
            if (datVO == null)
            {
                await base.Execute(new ShowMessageCmd(
                    "The selected DAT's details cannot be parsed."));
            }
            else
            {
                if (base.UnitDisplayModel != null)
                {
                    base.DatDetailsMediator?.SetView(
                        datVO
                        , base.UnitDisplayModel.GetUnit()
                        , base.UnitDisplayModel.GetShowUnitInCells());
                }

                await base.Execute(new SetActiveDatModel(
                    datVO, SetActiveDatModel.LOCAL_DAT_MODEL_ENUM.Detailed));
            }

            return await Complete();
        }
    }
}
