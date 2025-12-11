using Lyralabs.TempMailServer.Data;
using Microsoft.JSInterop;
using CommunityToolkit.Mvvm.Messaging;
using Radzen;
using Radzen.Blazor;

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

        private RadzenDataList<MailPreviewDto> dataList;
        private List<MailPreviewDto> pagedMails = new();
        private int totalCount;
        private int pageSize = 20;
        private bool isFirstRender = true;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            this.MailboxSessionService.MailReceived += this.MailboxSessionService_MailReceived;
            await this.JsRuntime.InvokeVoidAsync("window.TempMailServer.InitializeAutoSelect");
            this.Messenger.Register(this);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender && isFirstRender)
            {
                isFirstRender = false;
                await LoadInitialData();
                StateHasChanged();
            }
        }

        private async Task LoadInitialData()
        {
            totalCount = await this.MailboxSessionService.GetTotalMailCount();
            pagedMails = await this.MailboxSessionService.GetPagedMails(0, pageSize);
        }

        private async void MailboxSessionService_MailReceived(object sender, EventArgs e)
        {
            await this.InvokeAsync(async () =>
            {
                if (dataList != null)
                {
                    await dataList.Reload();
                }
                else
                {
                    this.StateHasChanged();
                }
            });
        }

        private async Task LoadData(LoadDataArgs args)
        {
            totalCount = await this.MailboxSessionService.GetTotalMailCount();
            pagedMails = await this.MailboxSessionService.GetPagedMails(args.Skip ?? 0, args.Top ?? pageSize);
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
            
            if (dataList != null)
            {
                await dataList.Reload();
            }
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

        public async void Receive(MailReceivedMessage message)
        {
            await this.InvokeAsync(async () =>
            {
                if (dataList != null)
                {
                    await dataList.Reload();
                }
                else
                {
                    this.StateHasChanged();
                }
            });
        }

        public void Dispose()
        {
            this.Messenger.UnregisterAll(this);
            this.MailboxSessionService.MailReceived -= this.MailboxSessionService_MailReceived;
        }
    }
}
