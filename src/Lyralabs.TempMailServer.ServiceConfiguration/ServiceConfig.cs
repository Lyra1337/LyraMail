using System;
using Lyralabs.TempMailServer.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lyralabs.TempMailServer.ServiceConfiguration
{
    public class ServiceConfig
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DatabaseContext>(
                optionsAction: options => options.UseSqlite(configuration.GetConnectionString(nameof(DatabaseContext))),
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Singleton
            );
        }
    }
}
