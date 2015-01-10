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

        private string RawContent { get; set; }
        private StringBuilder Body { get; set; }
        private Dictionary<string, List<string>> Headers { get; set; }
        private MailServer Server { get; set; }

        [DataMember]
        public string Guid { get; private set; }

        [DataMember]
        public long Time { get; private set; }

        [DataMember]
        public DateTime ReceiveTime { get; private set; }

        [DataMember]
        public List<MailBodyPart> BodyParts { get; private set; }

        [DataMember]
        public string Recipient { get; private set; }

        [DataMember]
        public string Sender { get; private set; }

        [DataMember]
        public string Subject { get; private set; }

        public Mail(MailServer server, string rawContent)
        {
            if (String.IsNullOrEmpty(rawContent))
            {
                throw new ArgumentNullException("rawContent is null!");
            }

            this.Server = server;
            this.Time = DateTime.Now.Ticks - this.Server.StartTime.Ticks;
            this.ReceiveTime = DateTime.Now;

            if (rawContent != null)
            {
                this.RawContent = rawContent.Replace("\r", "");
            }

            this.ParseHeader();
            this.ParseBody();

            this.Guid = System.Guid.NewGuid().ToString();
        }

        private void ParseBody()
        {
            string contentType = null;
            string boundary = null;
            bool multipart = false;

            if (this.Headers.ContainsKey("Content-Type"))
            {
                List<string> c = this.Headers["Content-Type"];
                if (c != null && c.Count > 0)
                {
                    Match m = Mail.contentTypeParser.Match(c[0]);
                    if (m.Success)
                    {
                        boundary = m.Groups["boundary"].Value;
                        contentType = m.Groups["type"].Value;
                        multipart = true;
                    }
                }
            }

            if (multipart)
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

                foreach (string line in this.Body.Replace("\r", "").ToString().Split('\n'))
                {
                    if (line.StartsWith(String.Concat("--", boundary)))
                    {
                        if (line.EndsWith(String.Concat(boundary, "--")))
                            break;

                        if (parts == null)
                            parts = new List<List<string>>();

                        parts.Add(new List<string>());
                    }
                    else
                    {
                        if (parts == null)
                            continue;
                        parts[parts.Count - 1].Add(line);
                    }
                }

                foreach (List<string> part in parts)
                {
                    if (this.BodyParts == null)
                        this.BodyParts = new List<MailBodyPart>();

                    this.BodyParts.Add(new MailBodyPart(part, true));
                }
            }
            else
            {
                if (this.BodyParts == null)
                    this.BodyParts = new List<MailBodyPart>();

                this.Body.ToString();

                MailBodyPart part = new MailBodyPart(this.Body.ToString().Replace("\r", "").Split('\n').ToList(), false);

                if (this.Headers.ContainsKey("Content-Type"))
                    part.ContentType = this.Headers["Content-Type"][0];

                this.BodyParts.Add(part);
            }
        }

        private void ParseHeader()
        {
            this.Body = new StringBuilder();
            bool header = true;
            this.Headers = new Dictionary<string, List<string>>();

            foreach (string line in this.RawContent.Replace("\r", "").Split('\n'))
            {
                if (header)
                {
                    if (String.IsNullOrEmpty(line))
                    {
                        header = false;
                    }
                    else if (line.StartsWith(" ") && this.Headers.Count > 0)
                    {
                        this.Headers[this.Headers.Keys.Last()][this.Headers[this.Headers.Keys.Last()].Count - 1] += line.Substring(1, line.Length - 1).Trim();
                    }
                    else
                    {
                        string key, value;
                        int separator = line.IndexOf(':');
                        if (separator > 0)
                        {
                            key = line.Substring(0, separator);
                            value = line.Substring(separator + 1, line.Length - (separator + 1)).TrimStart();
                            if (this.Headers.ContainsKey(key) == false)
                            {
                                this.Headers.Add(key, new List<string>());
                            }
                            this.Headers[key].Add(value);
                        }
                    }
                }
                else
                {
                    this.Body.AppendLine(line);
                }
            }

            if (this.Headers.ContainsKey("From") && this.Headers["From"] != null && this.Headers["From"].Count > 0)
                this.Sender = this.Headers["From"][0].Trim();

            if (this.Headers.ContainsKey("To") && this.Headers["To"] != null && this.Headers["To"].Count > 0)
                this.Recipient = this.Headers["To"][0].Trim();

            if (this.Headers.ContainsKey("Subject") && this.Headers["Subject"] != null && this.Headers["Subject"].Count > 0)
                this.Subject = this.Headers["Subject"][0].Trim();
        }
    }
}