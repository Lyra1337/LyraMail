using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

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
        Console.WriteLine("Connected! [IP: {0}]", clientSocket.Client.RemoteEndPoint);
        NetworkStream stream = clientSocket.GetStream();
        StreamReader sr = new StreamReader(stream);
        while (sr.EndOfStream == false)
        {
          this.RunCommand(sr.ReadLine());
        }
        Console.WriteLine("--- EOS ---");
      }
    }

    private void RunCommand(string command)
    {
      Console.WriteLine(command);
    }
  }
}