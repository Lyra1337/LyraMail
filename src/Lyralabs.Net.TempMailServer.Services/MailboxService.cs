using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Lyralabs.Net.TempMailServer
{
    public class MailboxService
    {
        private readonly ConcurrentDictionary<string, MailboxDto> mails = new ConcurrentDictionary<string, MailboxDto>();
        private readonly ConcurrentDictionary<string, Action<EmailDto>> mailNotifications = new ConcurrentDictionary<string, Action<EmailDto>>();
        private readonly MailServerConfiguration mailServerConfiguration;
        private readonly AsymmetricCryptoService cryptoService;
        private readonly EmailCryptoService emailCryptoService;
        private readonly ILogger<MailboxService> logger;

        public MailboxService(
            MailServerConfiguration mailServerConfiguration,
            AsymmetricCryptoService cryptoService,
            EmailCryptoService emailCryptoService,
            ILogger<MailboxService> logger)
        {
            this.mailServerConfiguration = mailServerConfiguration;
            this.cryptoService = cryptoService;
            this.emailCryptoService = emailCryptoService;
            this.logger = logger;
        }

        public List<EmailDto> GetMails(string account, string privateKey)
        {
            if (String.IsNullOrWhiteSpace(account))
            {
                throw new ArgumentException($"'{nameof(account)}' cannot be null or whitespace.", nameof(account));
            }

            if (this.mails.ContainsKey(account) == true)
            {
                return this.mails[account].Mails
                    .Select(x => this.emailCryptoService.Decrypt(x, privateKey))
                    .ToList();
            }
            else
            {
                return new List<EmailDto>();
            }
        }

        public EmailDto GetMail(string account, Guid secret, string privateKey)
        {
            var mailbox = this.mails[account];

            var mail = mailbox.Mails.Single(x => x.Secret == secret);
            var decrypted = this.emailCryptoService.Decrypt(mail, privateKey);

            return decrypted;
        }

        public void RegisterForNewMails(string address, Action<EmailDto> handler)
        {
            // TODO: Add Multi Tab support
            this.mailNotifications[address.ToLower()] = handler;
        }

        public void UnregisterForNewMails(string address)
        {
            address = address.ToLower();

            if (this.mailNotifications.ContainsKey(address) == true)
            {
                this.mailNotifications.Remove(address, out _);
            }
        }

        public string GetOrCreateMailbox(string privateKey)
        {
            var publicKey = this.cryptoService.GetPublicKey(privateKey);

            var mailbox = this.mails.Values
                .Where(x => x.PublicKey == publicKey)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            return mailbox?.Address ?? this.GenerateNewMailbox(publicKey);
        }

        internal Task StoreMail(EmailDto mail)
        {
            if (mail is null)
            {
                throw new ArgumentNullException(nameof(mail));
            }

            if (mail.To?.Any() != true)
            {
                throw new ArgumentException($"{nameof(mail)}.{nameof(mail.To)} cannot be empty.");
            }

            foreach (var toAddress in mail.To)
            {
                var account = toAddress.Address;

                if (this.mails.TryGetValue(account, out var mailbox) == true)
                {
                    var encryptedMail = this.emailCryptoService.Encrypt(mail, mailbox.PublicKey);

                    mailbox.Mails.Insert(0, encryptedMail);
                }
                else
                {
                    this.logger.LogInformation($"received mail with no corresponding mailbox. From={mail.FromAddress}; To={toAddress.Address}");

                    this.mails[account] = new MailboxDto()
                    {
                        Mails = new List<EmailDto>()
                        {
                            mail
                        }
                    };
                }

                if (this.mailNotifications.ContainsKey(account.ToLower()) == true)
                {
                    this.mailNotifications[account.ToLower()](mail);
                }
            }

            return Task.CompletedTask;
        }

        public string GenerateNewMailbox(string publicKey)
        {
            string mailAddress;

            do
            {
                mailAddress = String.Concat(
                    Guid.NewGuid().ToString().Split('-').First(),
                    "@",
                    this.mailServerConfiguration.Domain
                );
            } while (this.mails.ContainsKey(mailAddress) == true);

            this.mails[mailAddress] = new MailboxDto()
            {
                Address = mailAddress,
                CreatedAt = DateTime.Now,
                PublicKey = publicKey
            };

            return mailAddress;
        }
    }
}
