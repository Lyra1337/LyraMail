using Blazored.LocalStorage;
using Lyralabs.TempMailServer.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Lyralabs.TempMailServer.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddBlazoredLocalStorage();
            services.AddSingleton<IMessenger>(x => WeakReferenceMessenger.Default);

            services.AddScoped<UserState>();

            services.AddSingleton(x => x.Resolve<IConfiguration>().GetSection("MailServer").Get<MailServerConfiguration>());
            services.AddSingleton<MailServerService>();
            services.AddHostedService(x => x.Resolve<MailServerService>());

            services.AddSingleton<MapperService>();
            services.AddSingleton(x => x.Resolve<MapperService>().Mapper);

            services.AddSingleton<MailboxService>();

            services.AddTransient<EmailCryptoService>();
            services.AddTransient<AsymmetricCryptoService>();

            services.AddTransient<MailRepository>();

            services.AddDbContext<DatabaseContext>(
                optionsAction: options => options.UseSqlite(this.Configuration.GetConnectionString(nameof(DatabaseContext))),
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Singleton
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            app.MigrateDatabase<DatabaseContext>();
        }
    }
}
