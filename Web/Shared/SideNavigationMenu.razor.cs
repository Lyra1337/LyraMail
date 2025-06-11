using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace Lyralabs.TempMailServer.Web.Shared
{
    partial class SideNavigationMenu : IDisposable
    {
        [Inject]
        protected IJSRuntime JsRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected MailboxSessionService MailboxSessionService { get; set; }

        [Inject]
        protected UserState UserState { get; set; }

        public bool IsMailboxActive { get; private set; }

        public int UnreadCount { get; private set; }

        protected override void OnParametersSet()
        {
            this.CheckMailboxUrl();
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            this.MailboxSessionService.MailReceived += this.MailboxSessionService_MailReceived;
            this.MailboxSessionService.MailRead += this.MailboxSessionService_MailRead;

            if (this.UserState.Secret is null)
            {
                this.UserState.Secret = await this.MailboxSessionService.GetOrCreateUserSecret();
            }

            if (this.UserState.CurrentMailbox is null)
            {
                await this.MailboxSessionService.GetMailbox();
            }
            else
            {
                await this.Refresh();
            }

            this.NavigationManager.LocationChanged += this.NavigationManager_LocationChanged;
        }

        private void MailboxSessionService_MailRead(object sender, EventArgs e)
        {
            this.UnreadCount = this.MailboxSessionService.Mails.Count(mail => mail.IsRead == false);
            this.InvokeAsync(this.StateHasChanged);
        }

        private void MailboxSessionService_MailReceived(object sender, EventArgs e)
        {
            this.UnreadCount = this.MailboxSessionService.Mails.Count(mail => mail.IsRead == false);
            _ = this.InvokeAsync(this.StateHasChanged);
        }

        private void NavigationManager_LocationChanged(object sender, LocationChangedEventArgs e)
        {
            _ = this.JsRuntime.InvokeVoidAsync("TempMailServer.CloseMenu");
            this.CheckMailboxUrl();
        }

        private void CheckMailboxUrl()
        {
            this.IsMailboxActive = Uri.TryCreate(this.NavigationManager.Uri, UriKind.Absolute, out var uri) == true
                && uri.LocalPath.Equals("/inbox", StringComparison.OrdinalIgnoreCase) == true;

            _ = this.InvokeAsync(this.StateHasChanged);
        }

        protected async Task Refresh()
        {
            await this.MailboxSessionService.Refresh();
        }

        protected void TestEmail()
        {
            this.MailboxSessionService.TestEmail();
        }

        protected async Task NewAddress()
        {
            var result = await this.JsRuntime.InvokeAsync<bool?>("confirm", "Are you sure you want to create a new mailbox? This will delete all your current emails.");

            if (result == true)
            {
                await this.MailboxSessionService.GetMailbox(forceNew: true);
            }
        }

        public void Dispose()
        {
            this.MailboxSessionService.MailReceived -= this.MailboxSessionService_MailReceived;
            this.MailboxSessionService.MailRead -= this.MailboxSessionService_MailRead;
        }
    }
}
