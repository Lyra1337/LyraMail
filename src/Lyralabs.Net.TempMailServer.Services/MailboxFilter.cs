using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace Lyralabs.Net.TempMailServer
{
    internal sealed class MailboxFilter : IMailboxFilter
    {
        private readonly MailServerConfiguration mailServerConfiguration;
        private readonly ILogger<MailboxFilter> logger;

        public MailboxFilter(MailServerConfiguration mailServerConfiguration, ILogger<MailboxFilter> logger)
        {
            this.mailServerConfiguration = mailServerConfiguration;
            this.logger = logger;
        }

        public Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size, CancellationToken cancellationToken)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }

        public Task<MailboxFilterResult> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from, CancellationToken cancellationToken)
        {
            this.logger.LogDebug($"checking if we can deliver mail from {from.AsAddress()} to {to.AsAddress()}");

            if (this.mailServerConfiguration.Domain.Equals(to.Host, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                this.logger.LogInformation($"accepting email from {from.AsAddress()} to {to.AsAddress()}");
                return Task.FromResult(MailboxFilterResult.Yes);
            }
            else
            {
                this.logger.LogInformation($"denying email from {from.AsAddress()} to {to.AsAddress()}");
                return Task.FromResult(MailboxFilterResult.NoTemporarily);
            }
        }
    }
}
