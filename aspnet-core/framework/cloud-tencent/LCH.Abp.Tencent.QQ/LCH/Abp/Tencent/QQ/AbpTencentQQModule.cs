using LCH.Abp.Tencent.Localization;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.Tencent.QQ;

[DependsOn(typeof(AbpTencentCloudModule))]
public class AbpTencentQQModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpTencentQQModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<TencentCloudResource>()
                .AddVirtualJson("/LCH/Abp/Tencent/QQ/Localization");
        });

        context.Services.AddAbpDynamicOptions<AbpTencentQQOptions, AbpTencentQQOptionsManager>();
    }
}
