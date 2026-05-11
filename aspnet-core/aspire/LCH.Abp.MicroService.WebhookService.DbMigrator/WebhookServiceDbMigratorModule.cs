using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.Abp.MicroService.WebhookService;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(WebhookServiceMigrationsEntityFrameworkCoreModule)
    )]
public class WebhookServiceDbMigratorModule : AbpModule
{
}
