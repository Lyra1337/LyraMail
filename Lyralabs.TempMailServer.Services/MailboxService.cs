using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lyralabs.TempMailServer.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Messaging;
using MimeKit;

namespace Lyralabs.TempMailServer
{
    public class MailboxService
    {
        private readonly MailServerConfiguration mailServerConfiguration;
        private readonly AsymmetricCryptoService asymmetricCryptoService;
        private readonly EmailCryptoService emailCryptoService;
        private readonly MailRepository mailRepository;
        private readonly IMessenger messenger;
        private readonly ILogger<MailboxService> logger;

        public MailboxService(
            MailServerConfiguration mailServerConfiguration,
            AsymmetricCryptoService asymmetricCryptoService,
            EmailCryptoService emailCryptoService,
            MailRepository mailRepository,
            IMessenger messenger,
            ILogger<MailboxService> logger)
        {
            this.mailServerConfiguration = mailServerConfiguration;
            this.asymmetricCryptoService = asymmetricCryptoService;
            this.emailCryptoService = emailCryptoService;
            this.mailRepository = mailRepository;
            this.messenger = messenger;
            this.logger = logger;
        }

        public async Task<List<MailModel>> GetDecryptedMailsAsync(string address, string privateKey)
        {
            if (String.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException($"'{nameof(address)}' cannot be null or whitespace.", nameof(address));
            }

            var mailbox = await this.mailRepository.GetMailbox(address, loadMails: true);

            if (mailbox is null)
            {
                return new List<MailModel>();
            }

            return mailbox.Mails
                .Select(x => this.emailCryptoService.Decrypt(x, privateKey))
                .ToList();
        }

        public async Task<MailModel> GetDecryptedMail(string account, Guid secret, string privateKey)
        {
            var mail = await this.mailRepository.GetMailBySecret(account, secret);

            if (mail is null)
            {
                throw new UnauthorizedAccessException();
            }

            var decrypted = this.emailCryptoService.Decrypt(mail, privateKey);

            return decrypted;
        }

        public async Task<string> GetOrCreateMailboxAsync(string privateKey, string password)
        {
            var publicKey = this.asymmetricCryptoService.GetPublicKey(privateKey);

            var mailBox = await this.mailRepository.GetMailboxByPublicKey(publicKey, password);

            if (mailBox is null)
            {
                return await this.GenerateNewMailbox(publicKey, password);
            }
            else
            {
                return mailBox.Address;
            }
        }

        public async Task DeleteMail(int id)
        {
            await this.mailRepository.DeleteMail(id);
        }

        internal async Task StoreMail(MailModel mail, InternetAddressList to)
        {
            if (mail is null)
            {
                throw new ArgumentNullException(nameof(mail));
            }

            if (to?.Any() != true)
            {
                throw new ArgumentException($"{nameof(mail)}.{nameof(to)} cannot be empty.");
            }

            List<MailboxModel> mailboxes = await this.mailRepository.GetMailboxes(to.OfType<MailboxAddress>().Select(x => x.Address).ToList());

            foreach (var recipient in to.OfType<MailboxAddress>().Select(x => x.Address))
            {
                var mailbox = mailboxes.SingleOrDefault(x => x.Address == recipient);

                if (mailbox != null)
                {
                    var encryptedMail = this.emailCryptoService.EncryptWithNewPassword(mail, mailbox.PublicKey);

                    encryptedMail.MailboxId = mailbox.Id;
                    await this.mailRepository.Insert(encryptedMail);
                }
                else
                {
                    this.logger.LogInformation($"disposing received mail with no corresponding mailbox. From={mail.FromAddress}; To={recipient}");
                }

                this.NotifyMail(mail, recipient);
            }
        }

        private void NotifyMail(MailModel mail, string recipient)
        {
            this.messenger.Send(new MailReceivedMessage(mail), recipient.ToLower());
        }

        public async Task<string> GenerateNewMailbox(string publicKey, string password)
        {
            string mailAddress;

            do
            {
                mailAddress = String.Concat(
                    Guid.NewGuid().ToString().Split('-').First(),
                    "@",
                    this.mailServerConfiguration.Domain
                );
            } while (await this.mailRepository.ExistsMailbox(mailAddress) == true);

            await this.mailRepository.CreateMailbox(mailAddress, publicKey, password);

            return mailAddress;
        }
    }
}
