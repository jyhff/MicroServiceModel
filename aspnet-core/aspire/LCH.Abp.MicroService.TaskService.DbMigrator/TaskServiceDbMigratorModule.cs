using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.Abp.MicroService.TaskService;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(TaskServiceMigrationsEntityFrameworkCoreModule)
    )]
public class TaskServiceDbMigratorModule : AbpModule
{
}
