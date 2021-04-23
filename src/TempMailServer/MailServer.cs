using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Lyralabs.Net.TempMailServer
{
    public class MailServer
    {
        private static readonly int SERVER_PORT = 25;

        protected TcpListener ServerSocket = null;

        public delegate void ChangedEventHandler(object sender, MailReceivedEventArgs e);
        public event ChangedEventHandler MailReceivedEvent = null;

        public List<Mail> Mails
        {
            get;
            set;
        }

        public DateTime StartTime
        {
            get;
            private set;
        }

        public MailServer()
        {
            this.Prepare();
        }

        private void Prepare()
        {
            this.Mails = new List<Mail>();
        }

        public void Run()
        {
            this.StartTime = DateTime.Now;
            this.ServerSocket = new TcpListener(IPAddress.Any, MailServer.SERVER_PORT);
            this.ServerSocket.Start();

            this.LogInformation(String.Format("Mailserver started at port {0}", MailServer.SERVER_PORT));

            while (true)
            {
                TcpClient clientSocket = this.ServerSocket.AcceptTcpClient();
                this.LogInformation(String.Format("Connected! [IP: {0}]", clientSocket.Client.RemoteEndPoint));

                Thread thread = new Thread(new ParameterizedThreadStart(this.ProcessConnection));
                thread.Start(clientSocket);
            }
        }

        private void LogInformation(string text)
        {
            // TODO: Add logging
        }

        private void ProcessConnection(object objTcpClient)
        {
            try
            {
                if (objTcpClient == null || objTcpClient is TcpClient == false)
                {
                    return;
                }

                TcpClient client = (TcpClient)objTcpClient;

                MailSession session = new MailSession(this, client);
                Mail mail = session.Run();

                if (mail == null)
                {
                    return;
                }

                this.MailReceived(mail);

                if (Directory.Exists("mails") == false)
                {
                    Directory.CreateDirectory("mails");
                }

                File.WriteAllText(String.Format("mails/email_{0}.json", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")), JsonConvert.SerializeObject(mail, Formatting.Indented));
            }
            catch (IOException)
            {
            }
        }

        private void MailReceived(Mail mail)
        {
            try
            {
                this.Mails.Add(mail);

                if (this.MailReceivedEvent != null)
                {
                    this.MailReceivedEvent(this, new MailReceivedEventArgs(mail));
                }
            }
            catch (Exception)
            {
            }
        }
    }
}