using LCH.Abp.Saas.MySql;
using LCH.Abp.Saas.Oracle;
using LCH.Abp.Saas.PostgreSql;
using LCH.Abp.Saas.Sqlite;
using LCH.Abp.Saas.SqlServer;
using Volo.Abp.Modularity;

namespace LCH.Abp.Saas;

[DependsOn(typeof(AbpSaasDomainModule))]
public class AbpSaasDbCheckerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpSaasConnectionStringCheckOptions>(options =>
        {
            options.ConnectionStringCheckers["mysql"] = new MySqlConnectionStringChecker();
            options.ConnectionStringCheckers["oracle"] = new OracleConnectionStringChecker();
            options.ConnectionStringCheckers["postgres"] = new NpgsqlConnectionStringChecker();
            options.ConnectionStringCheckers["sqlite"] = new SqliteConnectionStringChecker();
            options.ConnectionStringCheckers["sqlserver"] = new SqlServerConnectionStringChecker();
        });
    }
}
