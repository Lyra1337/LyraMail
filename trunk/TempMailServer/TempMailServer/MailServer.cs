using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

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
        NetworkStream stream = clientSocket.GetStream();
        Thread t = new Thread(new ParameterizedThreadStart(ProcessConnection));
        t.Start(stream);
      }
    }

    private void ProcessConnection(object s)
    {
      if (s is NetworkStream == false)
        return;

      NetworkStream stream = s as NetworkStream;

      StreamWriter sw = new StreamWriter(stream);
      StreamReader sr = new StreamReader(stream);
      Thread.Sleep(1000);
      sw.WriteLine("220 service ready");
      while (sr.EndOfStream == false)
      {
        this.RunCommand(sr.ReadLine());
      }
      Console.WriteLine("--- EOS ---");
    }

    private void RunCommand(string command)
    {
      Console.WriteLine(command);
    }
  }
}