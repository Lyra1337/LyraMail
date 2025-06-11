using System;

namespace Lyralabs.TempMailServer
{
    public class MailPreviewDto
    {
        public int Id { get; internal set; }
        public string Subject { get; internal set; }
        public string FromAddress { get; internal set; }
        public DateTime ReceivedDate { get; internal set; }
        public bool IsRead { get; set; }
        public string PreviewText { get; internal set; }
    }
}
