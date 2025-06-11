using Lyralabs.TempMailServer.Web.Messages;
using Microsoft.JSInterop;
using CommunityToolkit.Mvvm.Messaging;

namespace Lyralabs.TempMailServer.Web.Shared
{
    partial class TopNavigationMenu : IRecipient<UserStateChangedMessage>
    {
        [Inject]
        protected UserState UserState { get; set; }

        [Inject]
        protected IMessenger Messenger { get; set; }

        [Inject]
        protected IJSRuntime JsRuntime { get; set; }

        protected override void OnInitialized()
        {
            this.Messenger.Register(this);
        }

        public void Receive(UserStateChangedMessage message)
        {
            _ = this.InvokeAsync(() => this.StateHasChanged());
        }

        protected async Task CopyEmail()
        {
            try
            {
                await this.JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", this.UserState.CurrentMailbox);
            }
            catch { }
        }
    }
}
