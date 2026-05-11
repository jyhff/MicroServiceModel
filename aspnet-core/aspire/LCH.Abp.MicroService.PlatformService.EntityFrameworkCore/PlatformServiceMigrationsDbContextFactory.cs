using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LCH.Abp.MicroService.PlatformService;

public class PlatformServiceMigrationsDbContextFactory : IDesignTimeDbContextFactory<PlatformServiceMigrationsDbContext>
{
    public PlatformServiceMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("Default");

        var builder = new DbContextOptionsBuilder<PlatformServiceMigrationsDbContext>()
            .UseNpgsql(connectionString);

        return new PlatformServiceMigrationsDbContext(builder!.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../LCH.Abp.MicroService.PlatformService.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
