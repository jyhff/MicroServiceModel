using LCH.Abp.Gdpr.Localization;
using LCH.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.Gdpr.Identity;

[DependsOn(
    typeof(AbpGdprDomainModule),
    typeof(AbpIdentityDomainModule))]
public class AbpGdprDomainIdentityModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpGdprDomainIdentityModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<GdprResource>()
                .AddVirtualJson("/LCH/Abp/Gdpr/Identity/Localization/Resources");
        });

        Configure<AbpGdprOptions>(options =>
        {
            // 用户数据提供者
            options.GdprUserDataProviders.Add(new AbpGdprIdentityUserDataProvider());
            // 用户账户提供者
            options.GdprUserAccountProviders.Add(new AbpGdprIdentityUserAccountProvider());
        });
    }
}
