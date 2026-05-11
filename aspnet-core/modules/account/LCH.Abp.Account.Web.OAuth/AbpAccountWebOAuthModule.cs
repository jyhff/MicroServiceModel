using AspNet.Security.OAuth.Bilibili;
using AspNet.Security.OAuth.GitHub;
using AspNet.Security.OAuth.QQ;
using AspNet.Security.OAuth.Weixin;
using AspNet.Security.OAuth.WorkWeixin;
using LCH.Abp.Account.OAuth;
using LCH.Abp.Account.OAuth.Localization;
using LCH.Abp.Account.Web.OAuth.ExternalProviders.Bilibili;
using LCH.Abp.Account.Web.OAuth.ExternalProviders.GitHub;
using LCH.Abp.Account.Web.OAuth.ExternalProviders.QQ;
using LCH.Abp.Account.Web.OAuth.ExternalProviders.WeChat;
using LCH.Abp.Account.Web.OAuth.ExternalProviders.WeCom;
using LCH.Abp.Account.Web.OAuth.Microsoft.Extensions.DependencyInjection;
using LCH.Abp.Tencent.QQ;
using LCH.Abp.WeChat.Official;
using LCH.Abp.WeChat.Work;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account.Localization;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.Account.Web.OAuth;

[DependsOn(typeof(AbpAccountWebModule))]
[DependsOn(typeof(AbpAccountOAuthModule))]
[DependsOn(typeof(AbpTencentQQModule))]
[DependsOn(typeof(AbpWeChatOfficialModule))]
[DependsOn(typeof(AbpWeChatWorkModule))]
public class AbpAccountWebOAuthModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(typeof(AccountResource), typeof(AbpAccountWebOAuthModule).Assembly);
        });

        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(AbpAccountWebOAuthModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpAccountWebOAuthModule>("LCH.Abp.Account.Web.OAuth");
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<AccountResource>()
                .AddBaseTypes(typeof(AccountOAuthResource));
        });

        context.Services
            .AddAuthentication()
            .AddGitHub(options =>
            {
                options.ClientId = "ClientId";
                options.ClientSecret = "ClientSecret";

                options.Scope.Add("user:email");
            }).UseSettingProvider<
                GitHubAuthenticationOptions,
                GitHubAuthenticationHandler,
                GitHubAuthHandlerOptionsProvider>()
            .AddQQ(options =>
            {
                options.ClientId = "ClientId";
                options.ClientSecret = "ClientSecret";
            }).UseSettingProvider<
                QQAuthenticationOptions,
                QQAuthenticationHandler,
                QQAuthHandlerOptionsProvider>()
            .AddWeixin(options =>
            {
                options.ClientId = "ClientId";
                options.ClientSecret = "ClientSecret";
            }).UseSettingProvider<
                WeixinAuthenticationOptions,
                WeixinAuthenticationHandler,
                WeChatAuthHandlerOptionsProvider>()
            .AddWorkWeixin(options =>
            {
                options.ClientId = "ClientId";
                options.ClientSecret = "ClientSecret";
            }).UseSettingProvider<
                WorkWeixinAuthenticationOptions,
                WorkWeixinAuthenticationHandler,
                WeComAuthHandlerOptionsProvider>()
            .AddBilibili(options =>
            {
                options.ClientId = "ClientId";
                options.ClientSecret = "ClientSecret";
            }).UseSettingProvider<
                BilibiliAuthenticationOptions,
                BilibiliAuthenticationHandler,
                BilibiliAuthHandlerOptionsProvider>();
    }
}
