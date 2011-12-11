﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace Lyralabs.Net.TempMailServer
{
  public class MailSession
  {
    private static readonly Regex mailFromParser = new Regex("FROM:<([^>]+)>", RegexOptions.Compiled);
    private static readonly Regex mailToParser = new Regex("TO:<([^>]+)>", RegexOptions.Compiled);

    private TcpClient client = null;
    private NetworkStream stream = null;
    private StreamReader reader = null;
    private StreamWriter writer = null;

    private bool mailInput = false;
    private StringBuilder mailBody = null;

    private string sender = null;
    private string recipient = null;

    public MailSession(TcpClient _client)
    {
      this.client = _client;
      this.stream = _client.GetStream();
    }

    public void Run()
    {
      this.writer = new StreamWriter(this.stream);
      this.writer.AutoFlush = true;
      this.reader = new StreamReader(this.stream);
      this.writer.WriteLine("220 service ready");
      while (this.client.Connected && this.reader.EndOfStream == false)
      {
        this.RunCommand(this.reader.ReadLine());
      }
      Console.WriteLine("--- EOS ---");

      Mail body = new Mail(this.mailBody.ToString());
    }

    private void RunCommand(string command)
    {
      if (this.mailInput)
      {
        if (command == ".")
        {
          this.mailInput = false;
          this.writer.WriteLine("250 OK");
          Console.WriteLine(this.mailBody.ToString());
          return;
        }
        this.mailBody.AppendLine(command);
        return;
      }
      string[] tokens = command.Split(' ');
      if (tokens.Length > 0)
      {
        switch (tokens[0])
        {
          case "HELO":
            this.writer.WriteLine("250 OK"); //lyra.bz Hello " + tokens[1]);
            break;
          case "MAIL":
            this.sender = mailFromParser.Match(tokens[1]).Groups[1].Value;
            this.writer.WriteLine("250 OK");
            break;
          case "RCPT":
            this.recipient = mailToParser.Match(tokens[1]).Groups[1].Value;
            this.writer.WriteLine("250 OK");
            break;
          case "DATA":
            this.writer.WriteLine("354 start mail input");
            this.mailBody = new StringBuilder();
            this.mailInput = true;
            break;
          case "QUIT":
            this.writer.WriteLine("221 closing channel");
            break;
          default:
            this.writer.WriteLine("500 Command not recognized: " + tokens[0]);
            break;
        }
      }
      Console.WriteLine(command);
    }
  }
}