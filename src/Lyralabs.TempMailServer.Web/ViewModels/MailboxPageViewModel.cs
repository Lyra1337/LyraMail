using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Lyralabs.TempMailServer.Data;
using Microsoft.AspNetCore.Components;

namespace Lyralabs.TempMailServer.Web.ViewModels
{
    public class MailboxPageViewModel : ComponentBase, IDisposable
    {
        [Inject]
        protected AsymmetricCryptoService CryptoService { get; set; }

        [Inject]
        protected MailboxService MailboxService { get; set; }

        [Inject]
        protected UserState UserState { get; set; }

        [Inject]
        protected ILocalStorageService LocalStorage { get; set; }

        protected List<MailModel> Mails { get; private set; } = new List<MailModel>();
        public MailModel CurrentMail { get; private set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender == false)
            {
                return;
            }

            if (this.UserState.Secret is null)
            {
                this.UserState.Secret = await this.GetOrCreateUserSecret();
            }

            if (this.UserState.CurrentMailbox is null)
            {
                this.GetMailbox();
            }
            else
            {
                this.Refresh();
            }

            this.MailboxService.RegisterForNewMails(this.UserState.CurrentMailbox, this.OnNewMailReceived);
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

        protected async Task GetMailbox(bool forceNew = false)
        {
            var userSecret = this.UserState.Secret.Value;

            if (forceNew == true)
            {
                this.MailboxService.UnregisterForNewMails(this.UserState.CurrentMailbox);
                this.UserState.CurrentMailbox = await this.MailboxService.GenerateNewMailbox(userSecret.PublicKey, userSecret.Password);
                this.MailboxService.RegisterForNewMails(this.UserState.CurrentMailbox, this.OnNewMailReceived);
            }
            else
            {
                this.UserState.CurrentMailbox = await this.MailboxService.GetOrCreateMailboxAsync(userSecret.PrivateKey, userSecret.Password);
            }

            await this.Refresh();
        }

        private void OnNewMailReceived(MailModel mail)
        {
            this.InvokeAsync(() => this.Refresh()); // TODO: use mail from event
        }

        protected async Task Refresh()
        {
            this.Mails = await this.MailboxService.GetDecryptedMailsAsync(this.UserState.CurrentMailbox, this.UserState.Secret.Value.PrivateKey);
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

        protected void ShowMail(MailModel mail)
        {
            this.CurrentMail = mail;
        }

        protected string Truncate(string text, int maxLength)
        {
            if (String.IsNullOrEmpty(text) == true)
            {
                return text;
            }

            if (text.Length <= maxLength)
            {
                return text;
            }
            else
            {
                return String.Concat(text.Substring(0, maxLength), "...");
            }
        }

        public void Dispose()
        {
            if (this.UserState.CurrentMailbox is not null)
            {
                this.MailboxService.UnregisterForNewMails(this.UserState.CurrentMailbox);
            }
        }
    }
}
