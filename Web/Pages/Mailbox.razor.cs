using Lyralabs.TempMailServer.Data;
using Microsoft.JSInterop;
using CommunityToolkit.Mvvm.Messaging;

namespace Lyralabs.TempMailServer.Web.Pages
{
    partial class Mailbox : IRecipient<MailReceivedMessage>, IDisposable
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

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            this.MailboxSessionService.MailReceived += this.MailboxSessionService_MailReceived;
            await this.JsRuntime.InvokeVoidAsync("window.TempMailServer.InitializeAutoSelect");
            this.Messenger.Register(this);
        }

        private void MailboxSessionService_MailReceived(object sender, EventArgs e)
        {
            _ = this.InvokeAsync(this.StateHasChanged);
        }

        protected async Task ShowMail(MailPreviewDto mailPreview)
        {
            this.CurrentMail = await this.MailboxSessionService.GetMailByIdAsync(mailPreview.Id);
            mailPreview.IsRead = true;
            await this.MailboxSessionService.SetMailReadMark(mailPreview.Id, true);
        }

        protected async Task DeleteCurrentMail()
        {
            await this.MailboxSessionService.DeleteMail(this.CurrentMail.Id);
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

        public void Dispose()
        {
            this.Messenger.UnregisterAll(this);
            this.MailboxSessionService.MailReceived -= this.MailboxSessionService_MailReceived;
        }
    }
}
