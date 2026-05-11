using LCH.Abp.TaskManagement.HttpApi.Client;
using Volo.Abp.Modularity;

namespace LCH.Abp.BackgroundTasks.TaskManagement;

[DependsOn(typeof(AbpBackgroundTasksModule))]
[DependsOn(typeof(TaskManagementHttpApiClientModule))]
public class AbpBackgroundTasksTaskManagementModule : AbpModule
{

}
