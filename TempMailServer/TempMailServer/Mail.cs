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
        public string Guid
        {
            get;
            set;
        }

        [DataMember]
        public long Time
        {
            get;
            set;
        }

        [DataMember]
        public List<MailBodyPart> BodyParts
        {
            get;
            set;
        }

        [DataMember]
        public string Recipient
        {
            get;
            set;
        }

        [DataMember]
        public string Sender
        {
            get;
            set;
        }

        [DataMember]
        public string Subject
        {
            get;
            set;
        }

        public Mail(MailServer _server, string _rawContent)
        {
            if (String.IsNullOrEmpty(_rawContent))
                throw new ArgumentNullException("rawContent is null!");

            this.Time = DateTime.Now.Ticks - MailServer.StartTime.Ticks;

            this.server = _server;
            if (_rawContent != null)
                this.rawContent = _rawContent.Replace("\r", "");
            this.ParseHeader();
            this.ParseBody();

            this.Guid = System.Guid.NewGuid().ToString();
        }

        private void ParseBody()
        {
            string contentType = null;
            string boundary = null;
            bool multipart = false;

            if (this.headers.ContainsKey("Content-Type"))
            {
                List<string> c = this.headers["Content-Type"];
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

                foreach (string line in this.body.Replace("\r", "").ToString().Split('\n'))
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

                this.body.ToString();

                MailBodyPart part = new MailBodyPart(this.body.ToString().Replace("\r", "").Split('\n').ToList(), false);

                if (this.headers.ContainsKey("Content-Type"))
                    part.ContentType = this.headers["Content-Type"][0];

                this.BodyParts.Add(part);
            }
        }

        private void ParseHeader()
        {
            this.body = new StringBuilder();
            bool header = true;
            this.headers = new Dictionary<string, List<string>>();

            foreach (string line in this.rawContent.Replace("\r", "").Split('\n'))
            {
                if (header)
                {
                    if (String.IsNullOrEmpty(line))
                    {
                        header = false;
                    }
                    else if (line.StartsWith(" ") && this.headers.Count > 0)
                    {
                        this.headers[this.headers.Keys.Last()][this.headers[this.headers.Keys.Last()].Count - 1] += line.Substring(1, line.Length - 1).Trim();
                    }
                    else
                    {
                        string key, value;
                        int separator = line.IndexOf(':');
                        if (separator > 0)
                        {
                            key = line.Substring(0, separator);
                            value = line.Substring(separator + 1, line.Length - (separator + 1)).TrimStart();
                            if (this.headers.ContainsKey(key) == false)
                            {
                                this.headers.Add(key, new List<string>());
                            }
                            this.headers[key].Add(value);
                        }
                    }
                }
                else
                {
                    this.body.AppendLine(line);
                }
            }

            if (this.headers.ContainsKey("From") && this.headers["From"] != null && this.headers["From"].Count > 0)
                this.Sender = this.headers["From"][0].Trim();

            if (this.headers.ContainsKey("To") && this.headers["To"] != null && this.headers["To"].Count > 0)
                this.Recipient = this.headers["To"][0].Trim();

            if (this.headers.ContainsKey("Subject") && this.headers["Subject"] != null && this.headers["Subject"].Count > 0)
                this.Subject = this.headers["Subject"][0].Trim();
        }
    }
}