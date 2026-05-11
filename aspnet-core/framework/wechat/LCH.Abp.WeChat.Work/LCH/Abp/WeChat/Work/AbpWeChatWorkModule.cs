using LCH.Abp.Features.LimitValidation;
using LCH.Abp.WeChat.Common.Localization;
using LCH.Abp.WeChat.Work.Common;
using LCH.Abp.WeChat.Work.Localization;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.Caching;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.WeChat.Work;

[DependsOn(
    typeof(AbpCachingModule),
    typeof(AbpExceptionHandlingModule),
    typeof(AbpFeaturesLimitValidationModule),
    typeof(AbpSettingsModule),
    typeof(AbpWeChatWorkCommonModule))]
public class AbpWeChatWorkModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpWeChatWorkModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<WeChatWorkResource>("zh-Hans")
                .AddBaseTypes(typeof(WeChatCommonResource))
                .AddVirtualJson("/LCH/Abp/WeChat/Work/Localization/Resources");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace(WeChatWorkErrorCodes.Namespace, typeof(WeChatWorkResource));
        });

        context.Services.AddApiClient();
        context.Services.AddOAuthClient();
        context.Services.AddLoginClient();
    }
}
