using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Json;

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
      if(ctx is HttpListenerContext == false)
        throw new ArgumentException("no context given!");

      HttpListenerContext context = ctx as HttpListenerContext;
      HttpListenerRequest request = context.Request;
      HttpListenerResponse response = context.Response;

      try
      {
        string path = String.Concat(LOCAL_PATH, request.Url.AbsolutePath);
        
        if(path.EndsWith("/api.json"))
        {
          Dictionary<string, string> postParams = new Dictionary<string, string>();
          string postRequest = new StreamReader(request.InputStream).ReadToEnd();

          foreach(string s in postRequest.Split('&'))
          {
            string[] keyVal = s.Split('=');
            if(keyVal.Length == 2 && postParams.ContainsKey(s.ToLower()) == false)
              postParams.Add(keyVal[0].ToLower(), keyVal[1]);
          }

          foreach(string s in request.QueryString.AllKeys)
          {
            if(postParams.ContainsKey(s.ToLower()) == false)
              postParams.Add(s.ToLower(), request.QueryString[s]);
          }

          if(postParams.ContainsKey("action"))
          {
            switch(postParams["action"])
            {
              case "getmails":
                {
                  string json = Serialize(this.mailServer.Mails);
                  WriteAndClose(json, response);
                }
                break;
            }
          }
        }
        else
        {
          if(Directory.Exists(path))
          {
            if(path.EndsWith("/") == false)
              path = String.Concat(path, "/");

          }
          else if(File.Exists(path))
          {
            byte[] file = File.ReadAllBytes(path);
          }
          else
          {
            response.StatusCode = 404;
            StreamWriter writer = new StreamWriter(response.OutputStream);
            writer.Write("not found.");
            writer.Flush();
            response.AddHeader("Content-Type", "text/plain");
            response.OutputStream.Close();
          }
        }
      }
      catch(Exception)
      {
      }
    }

    private static string Serialize(object obj)
    {
      DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
      MemoryStream ms = new MemoryStream();
      serializer.WriteObject(ms, obj);
      return Encoding.Default.GetString(ms.ToArray());
    }

    private static void WriteAndClose(string content, HttpListenerResponse response)
    {
      try
      {
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;
        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
      }
      catch(Exception)
      {
      }
    }
  }
}