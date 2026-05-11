using LCH.Abp.BackgroundTasks;
using LCH.Abp.Identity.Notifications;
using Volo.Abp.Identity.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.Identity.Jobs;

[DependsOn(typeof(AbpIdentityDomainModule))]
[DependsOn(typeof(AbpIdentityNotificationsModule))]
[DependsOn(typeof(AbpBackgroundTasksAbstractionsModule))]
public class AbpIdentityJobsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpIdentityJobsModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<IdentityResource>()
                .AddVirtualJson("/LCH/Abp/Identity/Jobs/Localization/Resources");
        });
    }
}
