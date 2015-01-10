using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace Lyralabs.Net.TempMailServer
{
    public class WebServer
    {
        private static readonly string LOCAL_PATH = "web";
        private HttpListener Server { get; set; }
        private MailServer MailServer { get; set; }
        private int Port { get; set; }

        public WebServer(MailServer mailServer, int port)
        {
            this.MailServer = mailServer;
            this.Port = port;
            this.Server = new HttpListener();
            this.Server.Prefixes.Add(String.Concat("http://*:", this.Port, "/"));
        }

        public void Start()
        {
            this.Server.Start();

            Console.WriteLine("Webserver started at port {0}", this.Port);

            while (true)
            {
                HttpListenerContext context = this.Server.GetContext();
                Thread thread = new Thread(new ParameterizedThreadStart(this.ProcessRequest));
                thread.Start(context);
            }
        }

        private void ProcessRequest(object ctx)
        {
            if (ctx is HttpListenerContext == false)
            {
                throw new ArgumentException("no context given!");
            }

            HttpListenerContext context = ctx as HttpListenerContext;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                string path = String.Concat(LOCAL_PATH, request.Url.AbsolutePath);
                
                if (path.EndsWith("/api.json") || path.EndsWith("/api.xml"))
                {
                    Dictionary<string, string> postParams = new Dictionary<string, string>();
                    string postRequest = new StreamReader(request.InputStream).ReadToEnd();

                    foreach (string s in postRequest.Split('&'))
                    {
                        string[] keyVal = s.Split('=');
                        if (keyVal.Length == 2 && postParams.ContainsKey(s.ToLower()) == false)
                        {
                            postParams.Add(keyVal[0].ToLower(), keyVal[1]);
                        }
                    }

                    foreach (string s in request.QueryString.AllKeys)
                    {
                        if (postParams.ContainsKey(s.ToLower()) == false)
                        {
                            postParams.Add(s.ToLower(), request.QueryString[s]);
                        }
                    }

                    if (postParams.ContainsKey("action"))
                    {
                        switch (postParams["action"])
                        {
                            case "getmails":
                                {
                                    string json = "[]";
                                    if (postParams.ContainsKey("timestamp"))
                                    {
                                        long time = 0;
                                        if (Int64.TryParse(postParams["timestamp"], out time))
                                        {
                                            if (this.MailServer.Mails != null)
                                            {
                                                List<Mail> num = this.MailServer.Mails.Where(mail => mail.Time > time).ToList();
                                                if (num != null)
                                                {
                                                    json = WebServer.Serialize(num);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            json = WebServer.Serialize(new { error = "'timestamp' is in a wrong Format!" });
                                        }
                                    }
                                    else
                                    {
                                        if (this.MailServer.Mails != null)
                                        {
                                            json = Serialize(this.MailServer.Mails);
                                        }
                                    }

                                    WebServer.WriteAndClose(json, response);
                                }

                                return;

                            case "getinitialdata":
                                {
                                    WebServer.WriteAndClose("[]", response);
                                }

                                return;

                            default:
                                WebServer.WriteAndClose("wrong action", response);
                                return;
                        }
                    }
                    else
                    {
                        WebServer.WriteAndClose("wrong parameters", response);
                        return;
                    }
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        if (path.EndsWith("/") == false)
                        {
                            path = String.Concat(path, "/");
                        }

                        if (File.Exists(String.Concat(path, "index.htm")))
                        {
                            string content = File.ReadAllText(String.Concat(path, "index.htm"), Encoding.UTF8);
                            WebServer.WriteAndClose(content, response);
                        }
                        else
                        {
                            foreach (string f in Directory.GetFiles(path, "index.*", SearchOption.TopDirectoryOnly))
                            {
                                string content = File.ReadAllText(f);
                                WebServer.WriteAndClose(content, response);
                            }

                            WebServer.NotFound(response);
                        }
                    }
                    else if (File.Exists(path))
                    {
                        string content = File.ReadAllText(path);
                        WebServer.WriteAndClose(content, response);
                    }
                    else
                    {
                        WebServer.NotFound(response);
                    }
                }
            }
            catch (Exception ex)
            {
                WebServer.WriteAndClose(String.Concat(ex.Message, "\r\n  Stacktrace:\r\n", ex.StackTrace), response);
            }
        }

        private static void NotFound(HttpListenerResponse response)
        {
            response.StatusCode = 404;
            StreamWriter writer = new StreamWriter(response.OutputStream);
            writer.Write("not found.");
            writer.Flush();
            response.AddHeader("Content-Type", "text/plain");
            response.OutputStream.Close();
        }

        private static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private static void WriteAndClose(string content, HttpListenerResponse response)
        {
            try
            {
                response.ContentEncoding = Encoding.UTF8;
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}