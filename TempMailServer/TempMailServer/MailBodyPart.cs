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
    public MailBodyPart(List<string> bodyLines)
    {
      this.ParseBody(bodyLines);
    }

    private void ParseBody(List<string> bodyLines)
    {
      bool isHeader = true;
      StringBuilder body = new StringBuilder();
      foreach(string line in bodyLines)
      {
        if(isHeader)
        {
          if(String.IsNullOrWhiteSpace(line))
          {
            isHeader = false;
          }
          else
          {
            if(line.StartsWith("Content-Type:"))
            {
              string[] parts = line.Split(' ');
              switch(parts.Length)
              {
                case 2:
                  this.MimeType = parts[1];
                  this.Encoding = null;
                  break;
                case 3:
                  this.MimeType = parts[1];
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
      this.BodyText = body.ToString();
    }

    [DataMember]
    public string MimeType
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
  }
}