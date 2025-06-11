namespace Lyralabs.TempMailServer.Web.Pages;

partial class Index
{
    [Inject]
    public required StatisticService StatisticService { get; set; }

    private int LargestMailbox { get; set; }
    private int TotalMailboxes { get; set; }
    private int TotalMails { get; set; }
    private int YesterdaysMailCount { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        (this.TotalMails, this.TotalMailboxes, this.LargestMailbox, this.YesterdaysMailCount) = await this.StatisticService.GetStatsAsync();
    }
}
