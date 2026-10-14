using Datinate.Shared.Dat;
using RMVC;
using System.Diagnostics;

namespace Datinate.Rmvc.Command
{
    internal class SetActiveDatModel : CommandBase<RVoid>
    {
        internal enum LOCAL_DAT_MODEL_ENUM
        {
            NOT_SET,
            Detailed
        }

        private readonly DatVO datVO;
        private readonly LOCAL_DAT_MODEL_ENUM localDatModelEnum;

        public SetActiveDatModel(DatVO datVO, LOCAL_DAT_MODEL_ENUM localDatModelEnum)
        {
            this.datVO = datVO;
            this.localDatModelEnum = localDatModelEnum;
        }

        protected override async Task<RVoid> Run()
        {
            switch(localDatModelEnum)
            {
                case LOCAL_DAT_MODEL_ENUM.Detailed:
                    if (base.ActiveDatsModel != null)
                        base.ActiveDatsModel.DetailedDat = datVO;
                    break;
                default:
                    // TODO:
                    Debug.WriteLine("LOCAL DAT TODO: " + localDatModelEnum);
                    break;
            }

            return await Complete();
        }
    }
}
