using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Lyralabs.Net.TempMailServer
{
  [DataContract]
  public class Mail
  {
    private static readonly Regex contentTypeParser = new Regex("(?<type>(multipart[^ ]+)) .*?boundary=(?<boundary>([^ ]+))", RegexOptions.Compiled);

    private string rawContent = null;
    private StringBuilder body = null;
    private Dictionary<string, List<string>> headers = null;

    private MailServer server = null;

    [DataMember]
    public List<MailBodyPart> BodyParts
    {
      get;
      set;
    }

    public Mail(MailServer _server, string _rawContent)
    {
      if(String.IsNullOrEmpty(_rawContent))
        throw new ArgumentNullException("rawContent is null!");

      this.server = _server;
      this.rawContent = _rawContent;
      this.ParseHeader();
      this.ParseBody();
    }

    private void ParseBody()
    {
      string contentType = null;
      string boundary = null;
      bool multipart = false;

      if(this.headers.ContainsKey("Content-Type"))
      {
        List<string> c = this.headers["Content-Type"];
        if(c != null && c.Count > 0)
        {
          Match m = Mail.contentTypeParser.Match(c[0]);
          if(m.Success)
          {
            boundary = m.Groups["boundary"].Value;
            contentType = m.Groups["type"].Value;
            multipart = true;
          }
        }
      }

      if(multipart)
      {
        #region comments
        /*      Dictionary<string, List<string>> lines = new Dictionary<string, List<string>>();
        string currentContentType = null;
        bool isHeader = false;
        int partCounter = 0;
        foreach(string line in this.body.ToString().Replace("\r", "").Split('\n'))
        {
          if(String.IsNullOrEmpty(line))
          {
            isHeader = false;
            if(currentContentType == null)
            {
              continue;
            }
            else
            {
              if(lines.ContainsKey(currentContentType) == false)
                lines.Add(currentContentType, new List<string>());

              lines[currentContentType].Add("");
            }
          }
          else
          {
            if(isHeader)
            {
              if(line.StartsWith("Content-Type:"))
              {
                string[] cType = line.Split(' ');
                if(cType.Length > 1)
                {
                  currentContentType = cType[1];
                }
              }
            }
            if(line.Trim() == String.Concat("--", boundary))
            {
              isHeader = true;

              if(lines.ContainsKey(currentContentType) == false)
                lines.Add(currentContentType, new List<string>());

              lines[currentContentType].Add("");
            }
            else if(line.Trim() == String.Concat("--", boundary, "--"))
            {
              break;
            }
          }
        }*/
        #endregion

        List<List<string>> parts = null;
        List<MailBodyPart> mailBodyParts = new List<MailBodyPart>();

        foreach(string line in this.body.ToString().Split('\n'))
        {
          if(line.StartsWith(String.Concat("--", boundary)))
          {
            if(line.EndsWith(String.Concat(boundary, "--")))
              break;

            if(parts == null)
              parts = new List<List<string>>();

            parts.Add(new List<string>());
          }
          else
          {
            if(parts == null)
              continue;
            parts[parts.Count - 1].Add(line);
          }
        }

        foreach(List<string> part in parts)
        {
          if(this.BodyParts == null)
            this.BodyParts = new List<MailBodyPart>();

          this.BodyParts.Add(new MailBodyPart(part));
        }
      }
    }

    private void ParseHeader()
    {
      this.body = new StringBuilder();
      bool header = true;
      this.headers = new Dictionary<string, List<string>>();

      foreach(string line in this.rawContent.Replace("\r", "").Split('\n'))
      {
        if(header)
        {
          if(String.IsNullOrEmpty(line))
          {
            header = false;
          }
          else if(line.StartsWith(" ") && this.headers.Count > 0)
          {
            this.headers[this.headers.Keys.Last()][this.headers[this.headers.Keys.Last()].Count - 1] += line.Substring(1, line.Length - 1).Trim();
          }
          else
          {
            string key, value;
            int separator = line.IndexOf(':');
            key = line.Substring(0, separator);
            value = line.Substring(separator + 1, line.Length - (separator + 1)).TrimStart();
            if(this.headers.ContainsKey(key) == false)
            {
              this.headers.Add(key, new List<string>());
            }
            this.headers[key].Add(value);
          }
        }
        else
        {
          this.body.AppendLine(line);
        }
      }
    }
  }
}