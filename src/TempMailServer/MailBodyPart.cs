using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Lyralabs.Net.TempMailServer
{
    [DataContract]
    public class MailBodyPart
    {
        public MailBodyPart(List<string> bodyLines, bool isMultipart)
        {
            this.MultiPart = isMultipart;
            this.ParseBody(bodyLines);
        }

        private void ParseBody(List<string> bodyLines)
        {
            if (this.MultiPart)
            {
                bool isHeader = true;
                StringBuilder body = new StringBuilder();
                foreach (string line in bodyLines)
                {
                    if (isHeader)
                    {
                        if (String.IsNullOrWhiteSpace(line))
                        {
                            isHeader = false;
                        }
                        else
                        {
                            if (line.StartsWith("Content-Type:"))
                            {
                                string[] parts = line.Split(' ');
                                switch (parts.Length)
                                {
                                    case 2:
                                        this.ContentType = parts[1];
                                        this.Encoding = null;
                                        break;
                                    case 3:
                                        this.ContentType = parts[1];
                                        this.Encoding = parts[2];
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        body.AppendLine(line);
                    }
                }
                this.BodyText = body.ToString().Trim();
            }
            else
            {
                StringBuilder body = new StringBuilder();
                foreach (string line in bodyLines)
                {
                    body.AppendLine(line);
                }
                this.BodyText = body.ToString().Trim();
            }
        }

        [DataMember]
        public string ContentType
        {
            get;
            set;
        }

        [DataMember]
        public string Encoding
        {
            get;
            set;
        }

        [DataMember]
        public string BodyText
        {
            get;
            set;
        }

        [DataMember]
        public bool MultiPart
        {
            get;
            set;
        }
    }
}