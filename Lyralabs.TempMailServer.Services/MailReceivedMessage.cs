using Lyralabs.TempMailServer.Data;

namespace Lyralabs.TempMailServer
{
    public class MailReceivedMessage
    {
        public MailModel Mail { get; }

        public MailReceivedMessage(MailModel mail)
        {
            this.Mail = mail;
        }
    }
}
