using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Threading;

namespace Lyralabs.Net.TempMailServer.Test
{
    class Program
    {
        static string[] addresses = new string[] { "steve@example.com", "jobs@example.com" };

        static void Main(string[] args)
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (string address in addresses)
                {
                    SmtpClient client = new SmtpClient("127.0.0.1");
                    client.Timeout = 1000;
                    MailMessage msg = GenerateMessage(address);
                    client.Send(msg);
                    Console.WriteLine("msg sent.");
                }
            }
        }

        static MailMessage GenerateMessage(string receiver)
        {
            MailAddress from = new MailAddress("steve@contoso.com", "Steve Ballmer");
            MailAddress to = new MailAddress(receiver, "Steve Jobs");
            MailMessage msg = new MailMessage(from, to);
            msg.Subject = "Hi, wie gehts?";
            msg.Body = "body blubb";
            return msg;
        }
    }
}