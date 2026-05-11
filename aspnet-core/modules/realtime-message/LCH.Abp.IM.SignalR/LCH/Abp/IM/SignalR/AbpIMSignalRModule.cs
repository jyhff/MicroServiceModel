using LCH.Abp.IM.Localization;
using LCH.Abp.IM.SignalR.Messages;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.IM.SignalR;

[DependsOn(
    typeof(AbpIMModule),
    typeof(AbpAspNetCoreSignalRModule))]
public class AbpIMSignalRModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpIMOptions>(options =>
        {
            options.Providers.Add<SignalRMessageSenderProvider>();
        });

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpIMSignalRModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<AbpIMResource>()
                .AddVirtualJson("/LCH/Abp/IM/SignalR/Localization/Resources");
        });
    }
}
