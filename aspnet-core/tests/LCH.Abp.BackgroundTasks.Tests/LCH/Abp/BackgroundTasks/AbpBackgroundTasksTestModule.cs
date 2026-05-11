using LCH.Abp.Tests;
using Volo.Abp.Modularity;

namespace LCH.Abp.BackgroundTasks;

[DependsOn(
    typeof(AbpBackgroundTasksModule),
    typeof(AbpTestsBaseModule))]
public class AbpBackgroundTasksTestModule : AbpModule
{
}
