using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ShowMessageCmd : CommandBase<RVoid>
    {
        private readonly string message;
        private readonly MessageTitleEnum messageTitle;

        public ShowMessageCmd(string message, MessageTitleEnum messageTitle = MessageTitleEnum.Error)
        {
            this.message = message;
            this.messageTitle = messageTitle;
        }

        public enum MessageTitleEnum 
        {
            Error
            , Ok
        }

        protected override async Task<RVoid> Run() 
        { 
            string title = "";

            switch (messageTitle) {
                
                case MessageTitleEnum.Error:
                    title = "There was a Problem";
                    break;
                
                case MessageTitleEnum.Ok:
                    title = "OK";
                    break;
            }

            if (base.Shell != null)
            {
                _ = await base.Shell.ShowMessageBox(title, message);
            }

            return await Complete();
        }
    }
}
