using LCH.Abp.UI.Navigation.VueVbenAdmin5;
using LCH.MicroService.Applications.Single.EntityFrameworkCore.MySql;
using LCH.MicroService.Applications.Single.EntityFrameworkCore.PostgreSql;
using LCH.MicroService.Applications.Single.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.MicroService.Applications.Single.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpUINavigationVueVbenAdmin5Module),
    typeof(SingleMigrationsEntityFrameworkCorePostgreSqlModule),
    typeof(SingleMigrationsEntityFrameworkCoreSqlServerModule),
    typeof(SingleMigrationsEntityFrameworkCoreMySqlModule)
    )]
public partial class SingleDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        ConfigureTiming(configuration);
    }
}
