using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lyralabs.TempMailServer.Web
{
    public static class ExtensionMethods
    {
        public static void MigrateDatabase<TContext>(this IApplicationBuilder app) where TContext : DbContext
        {
            using var context = app.ApplicationServices.GetService<TContext>();
            context.Database.Migrate();
        }
    }
}
