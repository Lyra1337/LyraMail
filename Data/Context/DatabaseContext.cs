using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lyralabs.TempMailServer.Data.Context
{
    public class DatabaseContext : IdentityDbContext<UserModel, IdentityRole<int>, int>
    {
        public DbSet<MailboxModel> Mailboxes { get; set; }

        public DbSet<MailModel> Mails { get; set; }

        public DatabaseContext(DbContextOptions options)
            : base(options)
        { }
    }
}
