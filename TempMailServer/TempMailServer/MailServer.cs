using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

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

      while(true)
      {
        TcpClient clientSocket = this.serverSocket.AcceptTcpClient();
        NetworkStream stream = clientSocket.GetStream();
      }
    }
  }
}