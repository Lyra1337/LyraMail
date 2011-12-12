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
    private static object mailLock = new object();
    
    public List<Mail> Mails
    {
      get;
      set;
    }

    public MailServer()
    {
      this.Mails = new List<Mail>();
    }

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
      Mail mail = session.Run();
      lock(MailServer.mailLock)
      {
        this.Mails.Add(mail);
      }
    }
  }
}