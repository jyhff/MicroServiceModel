using LCH.Abp.BackgroundTasks.Activities;
using LCH.Abp.BackgroundTasks.Localization;
using LCH.Abp.Notifications;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.BackgroundTasks.Notifications;

[DependsOn(
    typeof(AbpBackgroundTasksActivitiesModule),
    typeof(AbpNotificationsModule))]
public class AbpBackgroundTasksNotificationsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpBackgroundTasksNotificationsModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<BackgroundTasksResource>()
                .AddVirtualJson("/LCH/Abp/BackgroundTasks/Notifications/Localization/Resources");
        });
    }
}
