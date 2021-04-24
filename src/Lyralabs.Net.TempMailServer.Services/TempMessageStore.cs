using AutoMapper;
using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lyralabs.Net.TempMailServer
{
    internal sealed class TempMessageStore : MessageStore
    {
        private readonly MailboxService mailboxService;
        private readonly IMapper mapper;
        private readonly ILogger<TempMessageStore> logger;

        public TempMessageStore(MailboxService mailboxService, IMapper mapper, ILogger<TempMessageStore> logger)
        {
            this.mailboxService = mailboxService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public override async Task<SmtpResponse> SaveAsync(
            ISessionContext context,
            IMessageTransaction transaction,
            ReadOnlySequence<byte> buffer,
            CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();

            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                await stream.WriteAsync(memory, cancellationToken);
            }

            stream.Position = 0;

            var message = await MimeKit.MimeMessage.LoadAsync(stream, cancellationToken);

            this.logger.LogInformation($"received E-Mail from {String.Join(", ", message.From)}");

            var dto = this.mapper.Map<EmailDto>(message);

            await this.mailboxService.StoreMail(dto);

            return SmtpResponse.Ok;
        }
    }
}
