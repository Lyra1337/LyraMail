using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lyralabs.Net.TempMailServer
{
    class Program
    {
        static void Main(string[] args)
        {
            MailServer mailServer = new MailServer();
            //WebServer webServer = new WebServer(mailServer, 1337);

            //Thread t = new Thread(webServer.Start);
            //t.Start();
            mailServer.Run();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}