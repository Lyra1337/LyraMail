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
    private static readonly Regex mailFromParser = new Regex("FROM:<([^>]+)>", RegexOptions.Compiled);
    private static readonly Regex mailToParser = new Regex("TO:<([^>]+)>", RegexOptions.Compiled);
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
      sw.WriteLine("220 service ready");
      sw.Flush();
      while (sr.EndOfStream == false)
      {
        this.RunCommand(sr.ReadLine(), sw, sr);
      }
      Console.WriteLine("--- EOS ---");
    }

    private void RunCommand(string command, StreamWriter sw, StreamReader sr)
    {
      string[] tokens = command.Split(' ');
      if (tokens.Length > 0)
      {
        switch (tokens[0])
        {
          case "HELO":
            sw.WriteLine("250 OK"); //lyra.bz Hello " + tokens[1]);
            sw.Flush();
            break;
          case "MAIL":
            string sender = mailFromParser.Match(tokens[1]).Groups[1].Value;
            sw.WriteLine("250 OK");
            sw.Flush();
            break;
          case "RCPT":
            string recipient = mailToParser.Match(tokens[1]).Groups[1].Value;
            sw.WriteLine("250 OK");
            sw.Flush();
            break;
          case "DATA":
            sw.WriteLine("354 start mail input");
            sw.Flush();
            break;
          default:
            sw.WriteLine("500 Command not recognized: " + tokens[0]);
            sw.Flush();
            break;
        }
      }
      Console.WriteLine(command);
    }
  }
}