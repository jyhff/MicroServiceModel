using LCH.Abp.BackgroundTasks;
using LCH.Abp.Elasticsearch.Jobs.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Timing;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.Elasticsearch.Jobs;

[DependsOn(typeof(AbpTimingModule))]
[DependsOn(typeof(AbpElasticsearchModule))]
[DependsOn(typeof(AbpBackgroundTasksAbstractionsModule))]
public class AbpElasticsearchJobsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpElasticsearchJobsModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<ElasticsearchJobsResource>()
                .AddVirtualJson("/LCH/Abp/Elasticsearch/Jobs/Localization/Resources");
        });
    }
}