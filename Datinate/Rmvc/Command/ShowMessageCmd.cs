using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class ShowMessageCmd : RCommand 
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

        protected override void Run() 
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

            MessageBox.Show(message, title, MessageBoxButtons.OK
                , title == "OK" ? MessageBoxIcon.Information : MessageBoxIcon.Exclamation);
        }
    }
}
