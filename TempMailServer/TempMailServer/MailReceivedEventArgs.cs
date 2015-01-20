using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lyralabs.Net.TempMailServer
{
    public class MailReceivedEventArgs : EventArgs
    {
        public Mail ReceivedMail { get; private set; }

        public MailReceivedEventArgs(Mail mail)
        {
            this.ReceivedMail = mail;
        }
    }
}