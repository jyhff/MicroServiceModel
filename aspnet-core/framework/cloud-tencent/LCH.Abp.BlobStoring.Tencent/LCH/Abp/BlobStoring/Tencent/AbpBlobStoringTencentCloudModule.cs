using LCH.Abp.Tencent;
using LCH.Abp.Tencent.Localization;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BlobStoring;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.BlobStoring.Tencent;

[DependsOn(
    typeof(AbpBlobStoringModule),
    typeof(AbpTencentCloudModule))]
public class AbpBlobStoringTencentCloudModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpBlobStoringTencentCloudModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<TencentCloudResource>()
                .AddVirtualJson("/LCH/Abp/BlobStoring/Tencent/Localization");
        });

        context.Services.AddTenantOssClient();
    }
}