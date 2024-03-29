﻿using System;
using System.Threading.Tasks;
using Lyralabs.TempMailServer.Data;
using Microsoft.JSInterop;
using CommunityToolkit.Mvvm.Messaging;

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

        protected override void OnParametersSet()
        {
            this.MailboxSessionService.MailReceived += this.MailboxSessionService_MailReceived;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await this.JsRuntime.InvokeVoidAsync("window.TempMailServer.InitializeAutoSelect");
        }

        private void MailboxSessionService_MailReceived(object sender, EventArgs e)
        {
            _ = this.InvokeAsync(this.StateHasChanged);
        }

        protected async Task ShowMail(MailModel mail)
        {
            this.CurrentMail = mail;
            await this.MailboxService.SetMailReadMark(mail.Id, true);
            mail.IsRead = true;
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

        ~MailboxPageViewModel()
        {
            this.MailboxSessionService.MailReceived -= this.MailboxSessionService_MailReceived;
        }
    }
}
