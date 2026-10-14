using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetProjectLoaderDatExpressionsCmd : CommandBase<RVoid>
    {
        private readonly string expressionsXmlFullpath;

        public SetProjectLoaderDatExpressionsCmd(string expressionsXmlFullpath) 
        {
            this.expressionsXmlFullpath = expressionsXmlFullpath;
        }

        protected override async Task<RVoid> Run() 
        {
            base.ProjectLoaderMediator?.SetExpressionsFile(expressionsXmlFullpath);

            return await Complete();
        }
    }
}
