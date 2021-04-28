using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace Lyralabs.Net.TempMailServer.Web.ViewModels
{
    public class MailboxPageViewModel : ComponentBase
    {
        [Inject]
        protected AsymmetricCryptoService CryptoService { get; set; }

        [Inject]
        protected MailboxService MailboxService { get; set; }

        [Inject]
        protected UserState UserState { get; set; }

        [Inject]
        protected ILocalStorageService LocalStorage { get; set; }

        protected List<EmailDto> Mails { get; private set; } = new List<EmailDto>();
        public EmailDto CurrentMail { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            if (this.UserState.Secret is null)
            {
                this.UserState.Secret = await this.GetOrCreateUserSecret();
            }

            if (this.UserState.CurrentMailbox is null)
            {
                this.GetNewMailbox();
            }
        }

        private async Task<UserSecret> GetOrCreateUserSecret()
        {
            if (await this.LocalStorage.ContainKeyAsync("secret") == true)
            {
                return await this.LocalStorage.GetItemAsync<UserSecret>("secret");
            }
            else
            {
                var secret = this.CryptoService.GenerateUserSecret();

                await this.LocalStorage.SetItemAsync("secret", secret);

                return secret;
            }
        }

        protected void GetNewMailbox()
        {
            this.UserState.CurrentMailbox = this.MailboxService.GenerateNewMailbox(this.UserState.Secret.Value.PublicKey);
            this.Refresh();
        }

        protected void Refresh()
        {
            this.Mails = this.MailboxService.GetMails(this.UserState.CurrentMailbox, this.UserState.Secret.Value.PrivateKey);
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

        protected string Truncate(string text, int maxLength)
        {
            if (text.Length <= maxLength)
            {
                return text;
            }
            else
            {
                return String.Concat(text.Substring(0, maxLength), "...");
            }
        }
    }
}
