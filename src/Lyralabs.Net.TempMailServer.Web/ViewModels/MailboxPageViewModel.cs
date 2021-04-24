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

        [Inject]
        protected UserState UserState { get; set; }

        protected List<EmailDto> Mails { get; private set; } = new List<EmailDto>();
        public EmailDto CurrentMail { get; private set; }

        protected override void OnInitialized()
        {
            if (this.UserState.CurrentMailbox is null)
            {
                this.GetNewMailbox();
            }
        }

        protected void GetNewMailbox()
        {
            this.UserState.CurrentMailbox= this.MailboxService.GenerateNewMailbox();
            this.Refresh();
        }

        protected void Refresh()
        {
            this.Mails = this.MailboxService.GetMails(this.UserState.CurrentMailbox);
            this.StateHasChanged();
        }

        protected void TestEmail()
        {
            SmtpClient client = new SmtpClient("127.0.0.1");
            client.Timeout = 1000;

            MailAddress from = new MailAddress("steve@contoso.com", "Steve Ballmer");
            MailAddress to = new MailAddress(this.UserState.CurrentMailbox, "Steve Jobs");
            MailMessage msg = new MailMessage(from, to);
            msg.Subject = "Hi, wie gehts?";
            msg.Body = $"body blubb \r\n{System.Guid.NewGuid()}";

            client.Send(msg);
        }

        protected void ShowMail(EmailDto mail)
        {
            this.CurrentMail = mail;
        }
    }
}
