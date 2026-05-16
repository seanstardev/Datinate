using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using System.Diagnostics;
namespace com.RADIO.Datinate.RMVC
{
    internal class SetActiveDatModel : RCommand
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

        protected override void Run()
        {
            switch(localDatModelEnum)
            {
                case LOCAL_DAT_MODEL_ENUM.Detailed:
                    if (Facade.Instance?.ActiveDatsModel != null)
                        Facade.Instance.ActiveDatsModel.DetailedDat = datVO;
                    break;
                default:
                    // TODO:
                    Debug.WriteLine("LOCAL DAT TODO: " + localDatModelEnum);
                    break;
            }
        }
    }
}
