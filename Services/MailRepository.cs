using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lyralabs.TempMailServer.Data;
using Lyralabs.TempMailServer.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Lyralabs.TempMailServer
{
    public class MailRepository
    {
        private readonly IServiceProvider serviceProvider;

        public MailRepository(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<List<MailModel>> GetMails(int mailBoxId)
        {
            using var context = this.serviceProvider.Resolve<DatabaseContext>();

            var mails = await context.Mails
                .Where(x => x.MailboxId == mailBoxId)
                .ToListAsync();

            return mails;
        }

        public async Task<MailModel> GetMail(string account, int id)
        {
            account = this.NormalizeEmailAddress(account);

            using var context = this.serviceProvider.Resolve<DatabaseContext>();
            var mail = await context.Mails
                .Where(x => x.Mailbox.Address == account)
                .Where(x => x.Id == id)
                .SingleAsync();

            return mail;
        }

        public async Task<MailModel> GetMailBySecret(string account, Guid secret)
        {
            account = this.NormalizeEmailAddress(account);

            using var context = this.serviceProvider.Resolve<DatabaseContext>();
            var mail = await context.Mails.SingleAsync(x => x.Secret == secret && x.Mailbox.Address == account);

            return mail;
        }

        public async Task<MailboxModel> GetMailboxByPublicKey(string publicKey, string password)
        {
            using var context = this.serviceProvider.Resolve<DatabaseContext>();
            var passwordHash = this.HashPassword(password);
            return await context.Mailboxes
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.Password == passwordHash && x.PublicKey == publicKey);
        }

        public async Task<MailboxModel> GetMailbox(string address, bool loadMails)
        {
            address = this.NormalizeEmailAddress(address);

            using var context = this.serviceProvider.Resolve<DatabaseContext>();
            IQueryable<MailboxModel> query = context.Mailboxes;

            if (loadMails == true)
            {
                query = query.Include(x => x.Mails);
            }

            var mailbox = await query.SingleAsync(x => x.Address == address);

            mailbox.Mails = mailbox.Mails
                .OrderByDescending(x => x.ReceivedDate)
                .ToList();

            return mailbox;
        }

        public async Task<List<MailboxModel>> GetMailboxes(List<string> addresses)
        {
            using var context = this.serviceProvider.Resolve<DatabaseContext>();
            addresses = addresses.Select(x => this.NormalizeEmailAddress(x)).ToList();

            return await context.Mailboxes
                .Where(x => addresses.Contains(x.Address) == true)
                .ToListAsync();
        }

        internal async Task DeleteMail(int id)
        {
            using var context = this.serviceProvider.Resolve<DatabaseContext>();
            context.Entry(new MailModel() { Id = id }).State = EntityState.Deleted;
            await context.SaveChangesAsync();
        }

        public async Task<bool> ExistsMailbox(string mailAddress)
        {
            using var context = this.serviceProvider.Resolve<DatabaseContext>();
            mailAddress = this.NormalizeEmailAddress(mailAddress);
            return await context.Mailboxes.AnyAsync(x => x.Address == mailAddress);
        }

        public async Task CreateMailbox(string mailAddress, string publicKey, string password)
        {
            using var context = this.serviceProvider.Resolve<DatabaseContext>();
            mailAddress = this.NormalizeEmailAddress(mailAddress);

            context.Mailboxes.Add(new MailboxModel()
            {
                Address = mailAddress,
                PublicKey = publicKey,
                CreatedAt = DateTime.Now,
                Password = this.HashPassword(password)
            });

            await context.SaveChangesAsync();
        }

        public async Task Insert(MailModel encryptedMail)
        {
            using var context = this.serviceProvider.Resolve<DatabaseContext>();
            context.Mails.Add(encryptedMail);
            await context.SaveChangesAsync();
        }

        private string HashPassword(string password)
        {
            if (String.IsNullOrEmpty(password))
            {
                throw new ArgumentException($"'{nameof(password)}' cannot be null or empty.", nameof(password));
            }

            using (var sha = SHA512.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return String.Join(String.Empty, hash.Select(x => x.ToString("X2")));
            }
        }

        public string NormalizeEmailAddress(string address)
        {
            return address.Trim().ToLower();
        }

        public async Task SetReadMark(int mailId, bool isRead)
        {
            await using var context = this.serviceProvider.Resolve<DatabaseContext>();
            var entry = context.Entry(new MailModel()
            {
                Id = mailId,
                IsRead = isRead
            });
            
            entry.Property(x => x.IsRead).IsModified = true;

            await context.SaveChangesAsync();
        }
    }
}
