using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Lyralabs.TempMailServer.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Lyralabs.TempMailServer.Web.ViewModels
{
    public class MailboxPageViewModel : ComponentBase, IRecipient<MailReceivedMessage>
    {
        [Inject]
        protected AsymmetricCryptoService CryptoService { get; set; }

        [Inject]
        protected IMessenger Messenger { get; set; }

        [Inject]
        protected MailboxService MailboxService { get; set; }

        [Inject]
        protected UserState UserState { get; set; }

        [Inject]
        protected ILocalStorageService LocalStorage { get; set; }

        [Inject]
        protected IJSRuntime JsRuntime { get; set; }

        [Inject]
        protected WebServerConfiguration WebServerConfiguration { get; set; }

        protected List<MailModel> Mails { get; private set; } = new List<MailModel>();

        protected MailModel CurrentMail { get; private set; }

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
                await this.GetMailbox();
            }
            else
            {
                await this.Refresh();
            }

            await this.JsRuntime.InvokeVoidAsync("window.TempMailServer.InitializeAutoSelect");
        }

        private async Task<UserSecret> GetOrCreateUserSecret()
        {
            if (await this.LocalStorage.ContainKeyAsync("secret") == true)
            {
                var secretFromStorage = await this.LocalStorage.GetItemAsync<UserSecret>("secret");

                if (secretFromStorage.PrivateKey != null && secretFromStorage.PublicKey != null && secretFromStorage.Password != null)
                {
                    return secretFromStorage;
                }
            }

            var secret = this.CryptoService.GenerateUserSecret();

            await this.LocalStorage.SetItemAsync("secret", secret);

            return secret;
        }

        protected async Task GetMailbox(bool forceNew = false)
        {
            var userSecret = this.UserState.Secret.Value;

            if (forceNew == true)
            {
                this.UserState.CurrentMailbox = await this.MailboxService.GenerateNewMailbox(userSecret.PublicKey, userSecret.Password);
            }
            else
            {
                this.UserState.CurrentMailbox = await this.MailboxService.GetOrCreateMailboxAsync(userSecret.PrivateKey, userSecret.Password);
            }

            this.Messenger.Register(this, this.UserState.CurrentMailbox);

            await this.Refresh();
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
            msg.Body = $"body blubb \r\n{Guid.NewGuid()}";

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

        public void Receive(MailReceivedMessage message)
        {
            this.Mails.Insert(0, message.Mail);
            _ = this.InvokeAsync(this.StateHasChanged);
        }
    }
}
