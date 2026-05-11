using LCH.MicroService.AuthServer.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.MicroService.AuthServer.DbMigrator;

[DependsOn(
    typeof(AuthServerMigrationsEntityFrameworkCoreModule),
    typeof(AbpAutofacModule)
    )]
public partial class AuthServerDbMigratorModule : AbpModule
{
}
