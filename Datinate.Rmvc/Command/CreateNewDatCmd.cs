using Datinate.Shared.Dat;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class CreateNewDatCmd : RCommand
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
        protected override void Run() 
        {
            Facade.Instance?.CreateDatProxy?.createXmlAndSave(
                vo
                , fileFullpath
                , useMachineTags
            );
        }
    }
}
