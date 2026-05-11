using LCH.Abp.MicroService.LocalizationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LCH.Abp.MicroService.PlatformService;
public class LocalizationServiceMigrationsDbContextFactory : IDesignTimeDbContextFactory<LocalizationServiceMigrationsDbContext>
{
    public LocalizationServiceMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("Default");

        var builder = new DbContextOptionsBuilder<LocalizationServiceMigrationsDbContext>()
            .UseNpgsql(connectionString);

        return new LocalizationServiceMigrationsDbContext(builder!.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../LCH.Abp.MicroService.LocalizationService.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}

