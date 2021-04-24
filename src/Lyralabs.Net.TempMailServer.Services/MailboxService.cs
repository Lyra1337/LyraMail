using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lyralabs.Net.TempMailServer.Services
{
    public class MailboxService
    {
        private readonly ConcurrentDictionary<string, List<EmailDto>> mails = new ConcurrentDictionary<string, List<EmailDto>>();

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
                    mailList.Add(mail);
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
    }
}
