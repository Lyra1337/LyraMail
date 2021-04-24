using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Lyralabs.Net.TempMailServer.Web.ViewModels
{
    public class MailboxPageViewModel : ComponentBase
    {
        [Inject]
        protected MailboxService MailboxService { get; set; }

        protected List<EmailDto> Mails { get; private set; }

        protected string mailbox = null;

        protected override async Task OnInitializedAsync()
        {
            this.mailbox = this.MailboxService.GenerateNewMailbox();
        }

        protected void Refresh()
        {
            this.Mails = this.MailboxService.GetMails(this.mailbox);
            this.StateHasChanged();
        }

        protected void TestEmail()
        {
            SmtpClient client = new SmtpClient("127.0.0.1");
            client.Timeout = 1000;

            MailAddress from = new MailAddress("steve@contoso.com", "Steve Ballmer");
            MailAddress to = new MailAddress(this.mailbox, "Steve Jobs");
            MailMessage msg = new MailMessage(from, to);
            msg.Subject = "Hi, wie gehts?";
            msg.Body = "body blubb";

            client.Send(msg);
        }
    }
}
