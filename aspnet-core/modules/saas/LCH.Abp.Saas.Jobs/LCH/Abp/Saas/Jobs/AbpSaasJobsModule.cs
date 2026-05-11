using LCH.Abp.BackgroundTasks;
using LCH.Abp.Saas.Localization;
using Volo.Abp.Emailing;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.Saas.Jobs;

[DependsOn(typeof(AbpEmailingModule))]
[DependsOn(typeof(AbpSaasDomainModule))]
[DependsOn(typeof(AbpBackgroundTasksAbstractionsModule))]
public class AbpSaasJobsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpSaasJobsModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<AbpSaasResource>()
                .AddVirtualJson("/LCH/Abp/Saas/Jobs/Localization/Resources");
        });
    }
}
