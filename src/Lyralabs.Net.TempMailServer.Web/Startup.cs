using Blazored.LocalStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lyralabs.Net.TempMailServer.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddBlazoredLocalStorage();

            services.AddSingleton<MailboxService>();
            services.AddSingleton<MailServerService>();
            services.AddSingleton<MapperService>();
            services.AddSingleton(x => x.Resolve<MapperService>().Mapper);
            services.AddSingleton(x => x.Resolve<IConfiguration>().GetSection("MailServer").Get<MailServerConfiguration>());

            services.AddScoped<UserState>();

            services.AddTransient<EmailCryptoService>();
            services.AddTransient<AsymmetricCryptoService>();

            services.AddHostedService(x => x.Resolve<MailServerService>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            //app.UseSerilogRequestLogging();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
