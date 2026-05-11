using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.Abp.MicroService.MessageService;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(MessageServiceMigrationsEntityFrameworkCoreModule)
    )]
public class MessageServiceDbMigratorModule : AbpModule
{
}
