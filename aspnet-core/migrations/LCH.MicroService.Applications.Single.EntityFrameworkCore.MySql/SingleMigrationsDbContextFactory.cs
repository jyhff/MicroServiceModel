using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LCH.MicroService.Applications.Single.EntityFrameworkCore.MySql;

public class SingleMigrationsDbContextFactory : IDesignTimeDbContextFactory<SingleMigrationsDbContext>
{
    public SingleMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("Default");

        var builder = new DbContextOptionsBuilder<SingleMigrationsDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), b => b.MigrationsAssembly("LCH.MicroService.Applications.Single.EntityFrameworkCore.MySql"));

        return new SingleMigrationsDbContext(builder!.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),
                "../LCH.MicroService.Applications.Single.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.MySql.json", optional: true);

        return builder.Build();
    }
}
