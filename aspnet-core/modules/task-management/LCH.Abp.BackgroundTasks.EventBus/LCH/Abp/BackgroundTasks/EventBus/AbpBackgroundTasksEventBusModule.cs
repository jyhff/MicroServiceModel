using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace LCH.Abp.BackgroundTasks.EventBus;

[DependsOn(typeof(AbpEventBusModule))]
[DependsOn(typeof(AbpBackgroundTasksModule))]
public class AbpBackgroundTasksEventBusModule : AbpModule
{
}
