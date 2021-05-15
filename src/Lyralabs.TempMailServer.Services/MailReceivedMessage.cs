using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
