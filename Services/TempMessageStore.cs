using AutoMapper;
using Lyralabs.TempMailServer.Data;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lyralabs.TempMailServer
{
    public sealed class TempMessageStore : MessageStore
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
            try
            {
                await using var stream = new MemoryStream();

                var position = buffer.GetPosition(0);
                while (buffer.TryGet(ref position, out var memory))
                {
                    await stream.WriteAsync(memory, cancellationToken);
                }

                stream.Position = 0;

                var messages = await this.ParseMessage(stream, cancellationToken);

                var message = messages.First();

#if DEBUG
                this.logger.LogDebug($"storing E-Mail from {String.Join(", ", message.From)}");
#else
                this.logger.LogDebug($"storing E-Mail...");
#endif

                var dto = this.mapper.Map<MailModel>(message);

                await this.mailboxService.StoreMail(dto, message.To);

                return SmtpResponse.Ok;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "failed to store message");
                return SmtpResponse.TransactionFailed;
            }
        }

        private async Task<List<MimeMessage>> ParseMessage(MemoryStream stream, CancellationToken cancellationToken)
        {
            var parser = new MimeParser(stream);
            var messages = new List<MimeMessage>();
            this.logger.LogInformation("parsing messages...");

            while (parser.IsEndOfStream == false)
            {
                var message = await parser.ParseMessageAsync(cancellationToken);
                messages.Add(message);
                this.logger.LogInformation($"parsed message of type {message.GetType().Name}");

#if DEBUG
                this.logger.LogTrace($"TextBody: {message.TextBody}");
                this.logger.LogTrace($"HtmlBody: {message.HtmlBody}");
#endif
            }

            return messages;
        }
    }
}
