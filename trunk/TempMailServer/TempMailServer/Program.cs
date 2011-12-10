using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lyralabs.Net.TempMailServer
{
  class Program
  {
    static void Main(string[] args)
    {
      MailServer server = new MailServer();
      server.Start();
    }
  }
}