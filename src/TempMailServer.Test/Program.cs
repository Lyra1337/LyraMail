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
        static string[] addresses = new string[] { "steve@example.com", "jobs@example.com", "262427997691@tempmail.lyra.bz" };

        //static string mailserver = "127.0.0.1";
        static string mailserver = "116.203.154.142";
        //static string mailserver = "tempmail.lyra.bz";

        static void Main(string[] args)
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (string address in addresses)
                {
                    SmtpClient client = new SmtpClient(mailserver);
                    client.Timeout = 10000;
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