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
        public static DateTime StartTime
        {
            get;
            private set;
        }
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
            MailServer.StartTime = DateTime.Now;
            this.serverSocket = new TcpListener(IPAddress.Any, MailServer.SERVER_PORT);
            this.serverSocket.Start();

            Console.WriteLine("Mailserver started at port {0}", MailServer.SERVER_PORT);

            while (true)
            {
                TcpClient clientSocket = this.serverSocket.AcceptTcpClient();
                Console.WriteLine("Connected! [IP: {0}]", clientSocket.Client.RemoteEndPoint);
                Thread t = new Thread(new ParameterizedThreadStart(ProcessConnection));
                t.Start(clientSocket);
            }
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

                lock (MailServer.mailLock)
                {
                    this.Mails.Add(mail);
                }
            }
            catch (IOException ex)
            {
            }
        }
    }
}