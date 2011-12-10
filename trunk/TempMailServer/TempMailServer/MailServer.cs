using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace Lyralabs.Net.TempMailServer
{
  public class MailServer
  {
    private static readonly int SERVER_PORT = 25;
    protected TcpListener serverSocket = null;

    public void Start()
    {
      this.serverSocket = new TcpListener(IPAddress.Any, MailServer.SERVER_PORT);
      this.serverSocket.Start();

      while (true)
      {
        TcpClient clientSocket = this.serverSocket.AcceptTcpClient();
        Console.WriteLine("Connected! [IP: {0}]", clientSocket.Client.RemoteEndPoint);
        Thread t = new Thread(new ParameterizedThreadStart(ProcessConnection));
        t.Start(clientSocket);
      }
    }

    private void ProcessConnection(object s)
    {
      if (s is TcpClient == false)
        return;

      TcpClient client = s as TcpClient;

      MailSession session = new MailSession(client);
      session.Run();
    }
  }
}