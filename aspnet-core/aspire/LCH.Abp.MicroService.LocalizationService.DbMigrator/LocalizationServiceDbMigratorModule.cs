using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.Abp.MicroService.LocalizationService;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(LocalizationServiceMigrationsEntityFrameworkCoreModule)
    )]
public class LocalizationServiceDbMigratorModule : AbpModule
{
}
