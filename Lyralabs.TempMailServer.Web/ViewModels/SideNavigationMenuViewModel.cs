using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
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

        protected override void OnInitialized()
        {
            this.NavigationManager.LocationChanged += this.NavigationManager_LocationChanged;
        }

        private void NavigationManager_LocationChanged(object sender, LocationChangedEventArgs e)
        {
            _ = this.JsRuntime.InvokeVoidAsync("TempMailServer.CloseMenu");
        }
    }
}
