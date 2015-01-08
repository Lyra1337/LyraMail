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
        private HttpListener server = null;
        private MailServer mailServer = null;
        private int port = 8080;

        public WebServer(MailServer _mailServer, int _port)
        {
            this.mailServer = _mailServer;
            this.port = _port;
            this.server = new HttpListener();
            this.server.Prefixes.Add(String.Concat("http://*:", this.port, "/"));
        }

        public void Start()
        {
            this.server.Start();

            Console.WriteLine("Webserver started at port {0}", this.port);

            while (true)
            {
                HttpListenerContext context = this.server.GetContext();
                Thread t = new Thread(new ParameterizedThreadStart(this.ProcessRequest));
                t.Start(context);
            }
        }

        private void ProcessRequest(object ctx)
        {
            if (ctx is HttpListenerContext == false)
                throw new ArgumentException("no context given!");

            HttpListenerContext context = ctx as HttpListenerContext;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                string path = String.Concat(LOCAL_PATH, request.Url.AbsolutePath);

                bool wantsJson = false;
                if (path.EndsWith("json"))
                    wantsJson = true;

                if (path.EndsWith("/api.json") || path.EndsWith("/api.xml"))
                {
                    Dictionary<string, string> postParams = new Dictionary<string, string>();
                    string postRequest = new StreamReader(request.InputStream).ReadToEnd();

                    foreach (string s in postRequest.Split('&'))
                    {
                        string[] keyVal = s.Split('=');
                        if (keyVal.Length == 2 && postParams.ContainsKey(s.ToLower()) == false)
                            postParams.Add(keyVal[0].ToLower(), keyVal[1]);
                    }

                    foreach (string s in request.QueryString.AllKeys)
                    {
                        if (postParams.ContainsKey(s.ToLower()) == false)
                            postParams.Add(s.ToLower(), request.QueryString[s]);
                    }

                    if (postParams.ContainsKey("action"))
                    {
                        switch (postParams["action"])
                        {
                            case "getmails":
                                {
                                    string json = null;
                                    if (postParams.ContainsKey("timestamp"))
                                    {
                                        long time = 0;
                                        if (Int64.TryParse(postParams["timestamp"], out time))
                                        {
                                            if (this.mailServer.Mails != null)
                                            {
                                                IEnumerable<Mail> num = this.mailServer.Mails.Where(mail => mail.Time > time);
                                                if (num != null)
                                                {
                                                    json = Serialize(num.ToList(), wantsJson);
                                                }
                                                else
                                                {
                                                    json = "[]";
                                                }
                                            }
                                            else
                                            {
                                                json = "[]";
                                            }
                                        }
                                        else
                                        {
                                            json = "{\"error\":\"'timestamp' is in a wrong Format!\"}";
                                        }
                                    }
                                    else
                                    {
                                        if (this.mailServer.Mails != null)
                                        {
                                            json = Serialize(this.mailServer.Mails, wantsJson);
                                        }
                                        else
                                        {
                                            json = "[]";
                                        }
                                    }
                                    WriteAndClose(json, response);
                                }
                                return;
                            case "getinitialdata":
                                {

                                }
                                return;
                            default:
                                WriteAndClose("wrong action", response);
                                return;
                        }
                    }
                    else
                    {
                        WriteAndClose("wrong parameters", response);
                        return;
                    }
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        if (path.EndsWith("/") == false)
                            path = String.Concat(path, "/");

                        if (File.Exists(String.Concat(path, "index.htm")))
                        {
                            string content = File.ReadAllText(String.Concat(path, "index.htm"), Encoding.UTF8);
                            WriteAndClose(content, response);
                        }
                        else
                        {
                            foreach (string f in Directory.GetFiles(path, "index.*", SearchOption.TopDirectoryOnly))
                            {
                                string content = File.ReadAllText(f);
                                WriteAndClose(content, response);
                            }
                            NotFound(response);
                        }
                    }
                    else if (File.Exists(path))
                    {
                        string content = File.ReadAllText(path);
                        WriteAndClose(content, response);
                    }
                    else
                    {
                        NotFound(response);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteAndClose(String.Concat(ex.Message, "\r\n  Stacktrace:\r\n", ex.StackTrace), response);
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

        private static string Serialize(object obj, bool json)
        {
            MemoryStream ms = new MemoryStream();

            if (json)
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(obj.GetType());
                jsonSerializer.WriteObject(ms, obj);
            }
            else
            {
                DataContractSerializer xmlSerializer = new DataContractSerializer(obj.GetType());
                xmlSerializer.WriteObject(ms, obj);
            }

            return Encoding.Default.GetString(ms.ToArray());
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