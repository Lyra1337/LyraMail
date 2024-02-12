using Microsoft.EntityFrameworkCore;

namespace Lyralabs.TempMailServer.Data.Context
{
    public class DatabaseContext : IdentityDbContext<UserModel, DatabaseContext>
    {
        public DbSet<MailboxModel> Mailboxes { get; set; }

        public DbSet<MailModel> Mails { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        { }
    }
}
