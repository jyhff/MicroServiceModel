using LCH.MicroService.IdentityServer.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.MicroService.IdentityServer.DbMigrator;

[DependsOn(
    typeof(IdentityServerMigrationsEntityFrameworkCoreModule),
    typeof(AbpAutofacModule)
    )]
public partial class IdentityServerDbMigratorModule : AbpModule
{
}