using LCH.Abp.WeChat.Localization;
using LCH.Abp.WeChat.MiniProgram;
using LCH.Abp.WeChat.Official;
using LCH.Abp.WeChat.Work;
using LCH.Abp.WeChat.Work.Contacts;
using LCH.Abp.WeChat.Work.ExternalContact;
using LCH.Abp.WeChat.Work.Localization;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.WeChat.SettingManagement;

[DependsOn(
    typeof(AbpWeChatOfficialModule),
    typeof(AbpWeChatMiniProgramModule),
    typeof(AbpWeChatWorkModule),
    typeof(AbpWeChatWorkContactModule),
    typeof(AbpWeChatWorkExternalContactModule),
    typeof(AbpAspNetCoreMvcModule))]
public class AbpWeChatSettingManagementModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(AbpWeChatSettingManagementModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpWeChatSettingManagementModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<WeChatResource>()
                .AddVirtualJson("/LCH/Abp/WeChat/SettingManagement/Localization/Resources");
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<WeChatResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource),
                    typeof(WeChatWorkResource)
                );
        });
    }
}
