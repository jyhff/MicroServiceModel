using LCH.Abp.BackgroundTasks.Activities;
using LCH.Abp.BackgroundTasks.Localization;
using Volo.Abp.Emailing;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.BackgroundTasks.ExceptionHandling;

[System.Obsolete("Please use the `AbpBackgroundTasksNotificationsModule` module")]
[DependsOn(typeof(AbpBackgroundTasksActivitiesModule))]
[DependsOn(typeof(AbpEmailingModule))]
public class AbpBackgroundTasksExceptionHandlingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpBackgroundTasksExceptionHandlingModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<BackgroundTasksResource>()
                .AddVirtualJson("/LCH/Abp/BackgroundTasks/ExceptionHandling/Localization/Resources");
        });
    }
}
