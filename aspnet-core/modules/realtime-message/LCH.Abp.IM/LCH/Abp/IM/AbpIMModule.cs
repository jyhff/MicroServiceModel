using LCH.Abp.IM.Localization;
using LCH.Abp.RealTime;
using LCH.Abp.IdGenerator;
using Volo.Abp.EventBus;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.IM;

[DependsOn(
    typeof(AbpEventBusModule),
    typeof(AbpRealTimeModule),
    typeof(AbpLocalizationModule),
    typeof(AbpIdGeneratorModule))]
public class AbpIMModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpIMModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<AbpIMResource>()
                .AddVirtualJson("/LCH/Abp/IM/Localization/Resources");
        });
    }
}
