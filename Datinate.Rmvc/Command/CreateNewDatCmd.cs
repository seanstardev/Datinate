using Datinate.Shared.Dat;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class CreateNewDatCmd : CommandBase<RVoid>
    {
        private readonly DatVO vo;
        private readonly string fileFullpath;
        private readonly bool useMachineTags;

        public CreateNewDatCmd(DatVO vo, string fileFullpath, bool useMachineTags) 
        {
            this.vo = vo;
            this.fileFullpath = fileFullpath;
            this.useMachineTags = useMachineTags;
        }
        protected override async Task<RVoid> Run() 
        {
            base.CreateDatProxy?.CreateXmlAndSave(
                vo
                , fileFullpath
                , useMachineTags
            );

            return await Complete();
        }
    }
}
