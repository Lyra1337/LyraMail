﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace Lyralabs.Net.TempMailServer.Test
{
  class Program
  {
    static string[] addresses = new string[] { "test1@lyra.bz", "test2@lyra.bz", "test3@lyra.bz" };

    static void Main(string[] args)
    {
      SmtpClient client = new SmtpClient("127.0.0.1");

      for (int i = 0; i < 3; i++)
      {
        foreach (string address in addresses)
        {
          MailMessage msg = GenerateMessage(address);
          client.Send(msg);
          Console.WriteLine("msg sent.");
        }
      }
      
    }

    static MailMessage GenerateMessage(string receiver)
    {
      MailAddress from = new MailAddress("bla@contoso.com", "Steve Ballmer");
      MailAddress to = new MailAddress(receiver, "Steve Jobs");
      MailMessage msg = new MailMessage(from, to);
      msg.Subject = "Hi, wie gehts?";
      msg.Body = "body blubb";
      return msg;
    }
  }
}