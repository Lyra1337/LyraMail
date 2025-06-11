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

        protected async Task ShowMail(MailPreviewDto mailPreview)
        {
            this.CurrentMail = await this.MailboxSessionService.GetMailByIdAsync(mailPreview.Id);
            await this.MailboxService.SetMailReadMark(mailPreview.Id, true);
            mailPreview.IsRead = true;
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
            this.MailboxSessionService.MailReceived -= this.MailboxSessionService_MailReceived;
        }
    }
}
