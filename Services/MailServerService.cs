using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmtpServer;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lyralabs.TempMailServer
{
    public class MailServerService : IHostedService
    {
        private readonly ISmtpServerOptions options;
        private readonly TempMessageStore messageStore;
        private readonly MailboxFilter mailboxFilter;
        private readonly MailServerConfiguration mailServerConfiguration;
        private readonly ILogger<MailServerService> logger;
        private SmtpServer.SmtpServer smtpServer;

        public MailServerService(MailServerConfiguration mailServerConfiguration, TempMessageStore  tempMessageStore, MailboxFilter mailboxFilter, ILogger<MailServerService> logger)
        {
            this.mailServerConfiguration = mailServerConfiguration;
            this.logger = logger;
            this.messageStore = tempMessageStore;
            this.mailboxFilter = mailboxFilter;

            this.options = new SmtpServerOptionsBuilder()
                .ServerName(this.mailServerConfiguration.Domain)
                .Port(25, false)
                .CommandWaitTimeout(TimeSpan.FromSeconds(30))
                .Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            var mailServiceProvider = new SmtpServer.ComponentModel.ServiceProvider();
            mailServiceProvider.Add(this.messageStore);
            mailServiceProvider.Add(this.mailboxFilter);
            this.smtpServer = new SmtpServer.SmtpServer(this.options, mailServiceProvider);

            this.smtpServer.SessionCreated += this.SmtpServer_SessionCreated;
            this.smtpServer.SessionFaulted += this.SmtpServer_SessionFaulted;
            this.smtpServer.SessionCompleted += this.SmtpServer_SessionCompleted;

            _ = this.smtpServer.StartAsync(CancellationToken.None);
        }

        private void SmtpServer_SessionCompleted(object sender, SessionEventArgs e)
        {
            this.logger.LogInformation("Session completed.");
            e.Context.CommandExecuting -= this.Context_CommandExecuting;
        }

        private void SmtpServer_SessionCreated(object sender, SessionEventArgs e)
        {
            this.logger.LogInformation("Session created.");
            e.Context.CommandExecuted += this.Context_CommandExecuting;
        }

        private void Context_CommandExecuting(object sender, SmtpCommandEventArgs e)
        {
#if DEBUG
            this.logger.LogTrace($"Command executing: {e.Command.Name}");
#endif
        }

        private void SmtpServer_SessionFaulted(object sender, SessionFaultedEventArgs e)
        {
            this.logger.LogError($"Session Faulted. Properties: {String.Join(", ", e.Context?.Properties?.Select(x => x.Key) ?? Enumerable.Empty<string>())}");
            this.logger.LogError(e.Exception?.ToString());
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.smtpServer.Shutdown();
            await this.smtpServer.ShutdownTask;
        }
    }
}
