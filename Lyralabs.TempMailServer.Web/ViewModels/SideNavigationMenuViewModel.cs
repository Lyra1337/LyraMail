using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace Lyralabs.TempMailServer.Web.ViewModels
{
    public class SideNavigationMenuViewModel : ComponentBase
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

        protected override void OnParametersSet()
        {
            this.CheckMailboxUrl();
        }

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
                await this.MailboxSessionService.GetMailbox();
            }
            else
            {
                await this.Refresh();
            }
        }

        protected override void OnInitialized()
        {
            this.NavigationManager.LocationChanged += this.NavigationManager_LocationChanged;
        }

        private void NavigationManager_LocationChanged(object sender, LocationChangedEventArgs e)
        {
            _ = this.JsRuntime.InvokeVoidAsync("TempMailServer.CloseMenu");
            this.CheckMailboxUrl();
        }

        private void CheckMailboxUrl()
        {
            this.IsMailboxActive = Uri.TryCreate(this.NavigationManager.Uri, UriKind.Absolute, out Uri uri) == true
                && uri.LocalPath.Equals("/inbox", StringComparison.OrdinalIgnoreCase) == true;

            this.StateHasChanged();
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

        protected async Task NewAddress()
        {
            await this.MailboxSessionService.GetMailbox(forceNew: true);
        }
    }
}
