using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lyralabs.Net.TempMailServer
{
  class MailBody
  {
    private string rawContent;

    public MailBody(string _rawContent)
    {
      if (String.IsNullOrEmpty(_rawContent))
        throw new ArgumentNullException("rawContent is null!");

      this.rawContent = _rawContent;
      this.ParseHeader();
    }

    private void ParseHeader()
    {
      StringBuilder body = new StringBuilder();
      bool header = true;
      Dictionary<string, List<string>> headers = new Dictionary<string, List<string>>();
      foreach (string line in this.rawContent.Replace("\r", "").Split('\n'))
      {
        if (header)
        {
          if (String.IsNullOrEmpty(line))
          {
            header = false;
          }
          else if (line.StartsWith(" ") && headers.Count > 0)
          {
            headers[headers.Keys.Last()][headers[headers.Keys.Last()].Count - 1] += line.Substring(1, line.Length - 1).Trim();
          }
          else
          {
            string key, value;
            int separator = line.IndexOf(':');
            key = line.Substring(0, separator);
            value = line.Substring(separator + 1, line.Length - (separator + 1)).TrimStart();
            if (headers.ContainsKey(key) == false)
            {
              headers.Add(key, new List<string>());
            }
            headers[key].Add(value);
          }
        }
        else
        {
          body.AppendLine(line);
        }
      }
    }
  }
}
