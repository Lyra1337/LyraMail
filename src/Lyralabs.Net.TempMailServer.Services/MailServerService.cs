using Microsoft.Extensions.Hosting;
using SmtpServer;
using SmtpServer.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lyralabs.Net.TempMailServer.Services
{
    public class MailServerService : IHostedService
    {
        private readonly ISmtpServerOptions options;
        private readonly TempMessageStore messageStore;
        private readonly IServiceProvider serviceProvider;
        private SmtpServer.SmtpServer smtpServer;

        public MailServerService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            this.options = new SmtpServerOptionsBuilder()
                .ServerName("127.0.0.1")
                .Port(25, 587)
                .Build();

            this.messageStore = this.serviceProvider.Resolve<TempMessageStore>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(this.messageStore);
            this.smtpServer = new SmtpServer.SmtpServer(this.options, serviceProvider);
            _ = this.smtpServer.StartAsync(CancellationToken.None);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.smtpServer.Shutdown();
            await this.smtpServer.ShutdownTask;
        }
    }
}
