using LCH.Abp.TuiJuhe.Localization;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.TuiJuhe.SettingManagement;

[DependsOn(
    typeof(AbpTuiJuheModule),
    typeof(AbpAspNetCoreMvcModule))]
public class AbpTuiJuheSettingManagementModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(AbpTuiJuheSettingManagementModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpTuiJuheSettingManagementModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<TuiJuheResource>()
                .AddVirtualJson("/LCH/Abp/TuiJuhe/SettingManagement/Localization/Resources");
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<TuiJuheResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource)
                );
        });
    }
}
