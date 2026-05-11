using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.Abp.MicroService.PlatformService;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(PlatformServiceMigrationsEntityFrameworkCoreModule)
    )]
public class PlatformServiceDbMigratorModule : AbpModule
{
}
