using LCH.Abp.Identity.WeChat;
using LCH.Abp.WeChat.MiniProgram;
using LCH.Abp.WeChat.Official;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.OpenIddict.ExtensionGrantTypes;
using Volo.Abp.OpenIddict.Localization;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.OpenIddict.WeChat;

[DependsOn(
    typeof(AbpWeChatOfficialModule),
    typeof(AbpWeChatMiniProgramModule),
    typeof(AbpIdentityWeChatModule),
    typeof(AbpOpenIddictAspNetCoreModule))]
public class AbpOpenIddictWeChatModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<OpenIddictServerBuilder>(builder =>
        {
            builder
                .AllowWeChatFlow()
                .RegisterWeChatScopes()
                .RegisterWeChatClaims();
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpOpenIddictExtensionGrantsOptions>(options =>
        {
            options.Grants.TryAdd(
                WeChatTokenExtensionGrantConsts.OfficialGrantType,
                new WeChatOffcialTokenExtensionGrant());

            options.Grants.TryAdd(
                WeChatTokenExtensionGrantConsts.MiniProgramGrantType,
                new WeChatMiniProgramTokenExtensionGrant());
        });

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpOpenIddictWeChatModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<AbpOpenIddictResource>()
                .AddVirtualJson("/LCH/Abp/OpenIddict/WeChat/Localization/Resources");
        });
    }
}