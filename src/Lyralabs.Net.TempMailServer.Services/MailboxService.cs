using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lyralabs.Net.TempMailServer
{
    public class MailboxService
    {
        private readonly ConcurrentDictionary<string, List<EmailDto>> mails = new ConcurrentDictionary<string, List<EmailDto>>();
        private readonly MailServerConfiguration mailServerConfiguration;

        public MailboxService(MailServerConfiguration mailServerConfiguration)
        {
            this.mailServerConfiguration = mailServerConfiguration;
        }

        public List<EmailDto> GetMails(string account)
        {
            if (String.IsNullOrWhiteSpace(account))
            {
                throw new ArgumentException($"'{nameof(account)}' cannot be null or whitespace.", nameof(account));
            }

            if (this.mails.ContainsKey(account) == true)
            {
                return this.mails[account];
            }
            else
            {
                return new List<EmailDto>();
            }
        }

        public EmailDto GetMail(string account, Guid secret)
        {
            return this.mails[account].Single(x => x.Secret == secret);
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

                if (this.mails.TryGetValue(account, out var mailList) == true)
                {
                    mailList.Insert(0, mail);
                }
                else
                {
                    this.mails[account] = new List<EmailDto>()
                    {
                        mail
                    };
                }
            }

            return Task.CompletedTask;
        }

        public string GenerateNewMailbox()
        {
            string mailAddress;

            do
            {
                mailAddress = String.Concat(
                    Guid.NewGuid().ToString().Split('-').Last(),
                    "@",
                    this.mailServerConfiguration.Domain
                );
            } while (this.mails.ContainsKey(mailAddress) == true);

            this.mails[mailAddress] = new List<EmailDto>();

            return mailAddress;
        }
    }
}
