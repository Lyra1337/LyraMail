using System;
using System.Linq;
using System.Threading.Tasks;
using Lyralabs.TempMailServer.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Lyralabs.TempMailServer;

public class StatisticService(IDbContextFactory<DatabaseContext> databaseContextFactory)
{
    public async Task<(int TotalMails, int TotalMailboxes, int LargestMailbox, int YesterdaysMailCount)> GetStatsAsync()
    {
        using var context = await databaseContextFactory.CreateDbContextAsync();

        var totalMails = await context.Mails.CountAsync();
        var totalMailboxes = await context.Mailboxes.CountAsync();
        var largestMailbox = await context.Mailboxes
            .Select(m => m.Mails.Count)
            .MaxAsync();

        var yesterdaysMailCount = await context.Mails
            .Where(m => m.ReceivedDate.Date == DateTime.UtcNow.AddDays(-1).Date)
            .CountAsync();

        return (totalMails, totalMailboxes, largestMailbox, yesterdaysMailCount);
    }
}
