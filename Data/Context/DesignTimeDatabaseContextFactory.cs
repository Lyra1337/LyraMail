using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Lyralabs.TempMailServer.Data.Context
{
    class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var directory = new DirectoryInfo(Environment.CurrentDirectory);
            var configFile = new FileInfo(Path.Combine(directory.Parent.Parent.FullName, "Web", "appsettings.json"));

            var config = new ConfigurationBuilder()
                .AddJsonFile(configFile.FullName)
                .Build();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(config.GetConnectionString(nameof(DatabaseContext)))
                .Options;

            return new DatabaseContext(options);
        }
    }
}
