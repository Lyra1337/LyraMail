using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;

namespace Lyralabs.Net.TempMailServer
{
  class WebServer
  {
    private static readonly string LOCAL_PATH = "web";
    private HttpListener server = null;
    private int port = 8080;

    public WebServer(int _port)
    {
      this.port = _port;
      this.server = new HttpListener();
      this.server.Prefixes.Add(String.Concat("http://*:", this.port, "/"));
    }

    public void Start()
    {
      this.server.Start();
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

      string path = String.Concat(LOCAL_PATH, request.Url.AbsolutePath);

      if (path.EndsWith("/api.json"))
      {

      }

      if (Directory.Exists(path))
      {
        if (path.EndsWith("/") == false)
          path = String.Concat(path, "/");

      }
      else if (File.Exists(path))
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
}