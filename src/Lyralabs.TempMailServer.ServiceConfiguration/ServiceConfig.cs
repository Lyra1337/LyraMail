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
        }
    }
}
