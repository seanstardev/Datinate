using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetProjectLoaderDatExpressionsCmd:RCommand 
    {
        private readonly string expressionsXmlFullpath;

        public SetProjectLoaderDatExpressionsCmd(string expressionsXmlFullpath) 
        {
            this.expressionsXmlFullpath = expressionsXmlFullpath;
        }

        protected override void Run() 
        {
            Facade.Instance?.ProjectLoaderMediator?.SetExpressionsFile(expressionsXmlFullpath);
        }
    }
}
