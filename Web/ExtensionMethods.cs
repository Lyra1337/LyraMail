using Microsoft.EntityFrameworkCore;

namespace Lyralabs.TempMailServer.Web
{
    public static class ExtensionMethods
    {
        public static void MigrateDatabase<TContext>(this IApplicationBuilder app) where TContext : DbContext
        {
            var factory = app.ApplicationServices.GetService<IDbContextFactory<TContext>>();
            using var context = factory.CreateDbContext();
            context.Database.Migrate();
        }
    }
}
