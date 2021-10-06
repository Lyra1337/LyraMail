using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Lyralabs.TempMailServer.Data;
using Lyralabs.TempMailServer.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Lyralabs.TempMailServer.Web.ViewModels
{
    public class MailboxPageViewModel : ComponentBase, IRecipient<MailReceivedMessage>
    {
        [Inject]
        protected IMessenger Messenger { get; set; }

        [Inject]
        protected MailboxService MailboxService { get; set; }

        [Inject]
        protected UserState UserState { get; set; }

        [Inject]
        protected IJSRuntime JsRuntime { get; set; }

        [Inject]
        protected WebServerConfiguration WebServerConfiguration { get; set; }

        [Inject]
        protected MailboxSessionService MailboxSessionService { get; set; }

        protected MailModel CurrentMail { get; private set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender == false)
            {
                return;
            }

            if (this.UserState.Secret is null)
            {
                this.UserState.Secret = await this.MailboxSessionService.GetOrCreateUserSecret();
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
            await this.MailboxSessionService.Refresh();
            this.StateHasChanged();
        }

        protected void TestEmail()
        {
            this.MailboxSessionService.TestEmail();
        }

        protected void ShowMail(MailModel mail)
        {
            this.CurrentMail = mail;
        }

        protected async Task DeleteCurrentMail()
        {
            await this.MailboxSessionService.DeleteMail(this.CurrentMail);
            this.CurrentMail = null;
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
            _ = this.InvokeAsync(this.StateHasChanged);
        }
    }
}
