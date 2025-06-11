using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Lyralabs.TempMailServer.Data.Context;
using CommunityToolkit.Mvvm.Messaging;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;

namespace Lyralabs.TempMailServer.Web
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Serilog configuration
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "/logs/mailserver.log",
                    rollOnFileSizeLimit: true,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    retainedFileCountLimit: 10,
                    fileSizeLimitBytes: (int)Math.Pow(1024, 2) * 10) // 10 MB
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSingleton<IMessenger>(x => WeakReferenceMessenger.Default);

            builder.Services.AddScoped<UserState>();
            builder.Services.AddScoped<MailboxSessionService>();

            builder.Services.AddSingleton(builder.Configuration.GetSection("MailServer").Get<MailServerConfiguration>());
            builder.Services.AddSingleton(builder.Configuration.GetSection("WebServer").Get<WebServerConfiguration>());
            builder.Services.AddSingleton<MailServerService>();
            builder.Services.AddHostedService(x => x.GetRequiredService<MailServerService>());

            builder.Services.AddSingleton<MapperService>();
            builder.Services.AddSingleton(x => x.GetRequiredService<MapperService>().Mapper);

            builder.Services.AddSingleton<MailboxService>();

            builder.Services.AddTransient<EmailCryptoService>();
            builder.Services.AddTransient<AsymmetricCryptoService>();
            builder.Services.AddTransient<SymmetricCryptoService>();

            builder.Services.AddTransient<MailRepository>();

            builder.Services.AddDbContext<DatabaseContext>(
                options => options.UseSqlite(builder.Configuration.GetConnectionString(nameof(DatabaseContext))),
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Singleton
            );

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.MapControllers();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.MigrateDatabase<DatabaseContext>();

            app.Run();
        }
    }
}
