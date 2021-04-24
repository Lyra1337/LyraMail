using AutoMapper;
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

namespace Lyralabs.Net.TempMailServer.Services
{
    internal sealed class TempMessageStore : MessageStore
    {
        private readonly MailboxService mailboxService;
        private readonly IMapper mapper;

        public TempMessageStore(MailboxService mailboxService, IMapper mapper)
        {
            this.mailboxService = mailboxService;
            this.mapper = mapper;
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

            var dto = this.mapper.Map<EmailDto>(message);

            await this.mailboxService.StoreMail(dto);

            return SmtpResponse.Ok;
        }
    }
}
