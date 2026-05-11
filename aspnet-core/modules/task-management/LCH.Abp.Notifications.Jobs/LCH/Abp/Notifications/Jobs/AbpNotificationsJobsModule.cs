using LCH.Abp.BackgroundTasks;
using LCH.Abp.Notifications.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.Notifications.Jobs;

[DependsOn(typeof(AbpNotificationsModule))]
[DependsOn(typeof(AbpBackgroundTasksAbstractionsModule))]
public class AbpNotificationsJobsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpNotificationsJobsModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<NotificationsResource>()
                .AddVirtualJson("/LCH/Abp/Notifications/Jobs/Localization/Resources");
        });
    }
}
