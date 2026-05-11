using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.Abp.MicroService.AuthServer;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AuthServerMigrationsEntityFrameworkCoreModule))]
public class AuthServerDbMigratorModule : AbpModule
{
}
