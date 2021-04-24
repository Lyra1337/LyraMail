using SmtpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyralabs.Net.TempMailServer.Services
{
    class MailServerService
    {
        public MailServerService()
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Port(25, 587)
                .Build();
        }

        public void Start()
        {

        }
    }
}
