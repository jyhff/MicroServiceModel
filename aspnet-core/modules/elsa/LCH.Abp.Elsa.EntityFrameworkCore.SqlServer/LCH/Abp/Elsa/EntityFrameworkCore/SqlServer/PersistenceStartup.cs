using Elsa.Attributes;
using Microsoft.EntityFrameworkCore;

namespace LCH.Abp.Elsa.EntityFrameworkCore.SqlServer;

[Feature("DefaultPersistence:EntityFrameworkCore:SqlServer")]
public class PersistenceStartup : PersistenceStartupBase
{
    protected override string ProviderName => "SqlServer";

    protected override void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseSqlServer(connectionString);
    }
}
