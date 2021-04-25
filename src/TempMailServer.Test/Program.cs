using System;
using System.Net.Mail;

namespace Lyralabs.Net.TempMailServer.Test
{
    internal class Program
    {
        private static readonly string[] addresses = new string[] { "steve@example.com", "jobs@example.com", "767723dba2a2@tempmail.lyra.bz" };

        //static readonly string mailserver = "127.0.0.1";
        //static readonly string mailserver = "116.203.154.142";
        private static readonly string mailserver = "tempmail.lyra.bz";

        private static void Main(string[] args)
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

        private static MailMessage GenerateMessage(string receiver)
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