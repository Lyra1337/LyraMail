using Microsoft.Extensions.Hosting;
using SmtpServer;
using SmtpServer.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lyralabs.Net.TempMailServer
{
    public class MailServerService : IHostedService
    {
        private readonly ISmtpServerOptions options;
        private readonly TempMessageStore messageStore;
        private readonly IServiceProvider serviceProvider;
        private readonly MailServerConfiguration mailServerConfiguration;
        private SmtpServer.SmtpServer smtpServer;

        public MailServerService(IServiceProvider serviceProvider, MailServerConfiguration mailServerConfiguration)
        {
            this.serviceProvider = serviceProvider;
            this.mailServerConfiguration = mailServerConfiguration;
            this.options = new SmtpServerOptionsBuilder()
                .ServerName(mailServerConfiguration.Domain)
                .Port(25, 587)
                .CommandWaitTimeout(TimeSpan.FromSeconds(30))
                .Build();

            this.messageStore = this.serviceProvider.Resolve<TempMessageStore>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var mailServiceProvider = new ServiceProvider();
            mailServiceProvider.Add(this.messageStore);
            mailServiceProvider.Add(this.serviceProvider.Resolve<MailboxFilter>());
            this.smtpServer = new SmtpServer.SmtpServer(this.options, mailServiceProvider);
            _ = this.smtpServer.StartAsync(CancellationToken.None);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.smtpServer.Shutdown();
            await this.smtpServer.ShutdownTask;
        }
    }
}
