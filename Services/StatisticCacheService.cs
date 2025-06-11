using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lyralabs.TempMailServer
{
    public class StatisticCacheService(StatisticService statisticService, ILogger<StatisticCacheService> logger) : BackgroundService
    {
        public int LargestMailbox { get; private set; }
        public int TotalMailboxes { get; private set; }
        public int TotalMails { get; private set; }
        public int YesterdaysMailCount { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    (this.TotalMails, this.TotalMailboxes, this.LargestMailbox, this.YesterdaysMailCount) = await statisticService.GetStatsAsync();

                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while updating statistics cache.");

                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    { }
                }
            }
        }
    }
}
