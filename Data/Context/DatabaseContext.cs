using Microsoft.EntityFrameworkCore;

namespace Lyralabs.TempMailServer.Data.Context
{
    public class DatabaseContext : DbContext
    {
        public DbSet<MailboxModel> Mailboxes { get; set; }

        public DbSet<MailModel> Mails { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MailboxModel>()
                .HasIndex(m => m.Address);

            modelBuilder.Entity<MailModel>()
                .HasIndex(m => m.Secret);
        }
    }
}
