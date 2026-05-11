using LCH.Abp.BackgroundTasks;
using LCH.Abp.BackgroundTasks.Activities;
using LCH.Abp.BackgroundTasks.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace LCH.Abp.TaskManagement;

[DependsOn(typeof(TaskManagementDomainSharedModule))]
[DependsOn(typeof(AbpMapperlyModule))]
[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(AbpBackgroundTasksModule))]
[DependsOn(typeof(AbpBackgroundTasksEventBusModule))]
[DependsOn(typeof(AbpBackgroundTasksActivitiesModule))]
public class TaskManagementDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<TaskManagementDomainModule>();

        Configure<AbpDistributedEntityEventOptions>(options =>
        {
            options.EtoMappings.Add<BackgroundJobInfo, BackgroundJobEto>(typeof(TaskManagementDomainModule));

            options.AutoEventSelectors.Add<BackgroundJobInfo>();
        });

    }
}