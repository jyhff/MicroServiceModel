using LCH.Abp.Account;
using LCH.Abp.Account.Web.OAuth;
using LCH.Abp.Account.Web.OpenIddict;
using LCH.Abp.AspNetCore.HttpOverrides;
using LCH.Abp.AspNetCore.MultiTenancy;
using LCH.Abp.AspNetCore.Mvc.Wrapper;
using LCH.Abp.AuditLogging.Elasticsearch;
using LCH.Abp.BlobStoring.OssManagement;
using LCH.Abp.Data.DbMigrator;
using LCH.Abp.Emailing.Platform;
using LCH.Abp.EventBus.CAP;
using LCH.Abp.Exporter.MiniExcel;
using LCH.Abp.Gdpr;
using LCH.Abp.Gdpr.Web;
using LCH.Abp.Identity.AspNetCore.Session;
using LCH.Abp.Identity.OrganizaztionUnits;
using LCH.Abp.Identity.Session.AspNetCore;
using LCH.Abp.Localization.CultureMap;
using LCH.Abp.OpenIddict.AspNetCore.Session;
using LCH.Abp.OpenIddict.LinkUser;
using LCH.Abp.OpenIddict.Portal;
using LCH.Abp.OpenIddict.Sms;
using LCH.Abp.OpenIddict.WeChat;
using LCH.Abp.OpenIddict.WeChat.Work;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.Sms.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.Identity;

namespace LCH.Abp.MicroService.AuthServer;

[DependsOn(
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAccountWebOAuthModule),
    typeof(AbpBlobStoringOssManagementModule),
    typeof(AbpGdprApplicationModule),
    typeof(AbpGdprHttpApiModule),
    typeof(AbpGdprWebModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpIdentityAspNetCoreSessionModule),
    typeof(AbpOpenIddictAspNetCoreSessionModule),
    typeof(AbpIdentitySessionAspNetCoreModule),
    typeof(AbpOpenIddictSmsModule),
    typeof(AbpOpenIddictWeChatModule),
    typeof(AbpOpenIddictLinkUserModule),
    typeof(AbpOpenIddictPortalModule),
    typeof(AbpOpenIddictWeChatWorkModule),
    typeof(AbpIdentityOrganizaztionUnitsModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AuthServerMigrationsEntityFrameworkCoreModule),
    typeof(AbpDataDbMigratorModule),
    typeof(AbpAuditLoggingElasticsearchModule), // 放在 AbpIdentity 模块之后,避免被覆盖
    typeof(AbpLocalizationCultureMapModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreMvcWrapperModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpExporterMiniExcelModule),
    typeof(AbpEmailingPlatformModule),
    typeof(AbpSmsPlatformModule),
    typeof(AbpCAPEventBusModule)
    )]
public partial class AuthServerModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        PreConfigureWrapper();
        PreConfigureFeature();
        PreForwardedHeaders();
        PreConfigureAuthServer();
        PreConfigureApp(configuration);
        PreConfigureCAP(configuration);
        PreConfigureCertificate(configuration, hostingEnvironment);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureBranding(configuration);
        ConfigureCaching(configuration);
        ConfigureIdentity(configuration);
        ConfigureVirtualFileSystem();
        ConfigureFeatureManagement();
        ConfigureSettingManagement();
        ConfigurePermissionManagement();
        ConfigureLocalization();
        ConfigureDataSeeder();
        ConfigureUrls(configuration);
        ConfigureTiming(configuration);
        ConfigureAuditing(configuration);
        ConfigureAuthServer(configuration);
        ConfigureBlobStoring(configuration);
        ConfigureMultiTenancy(configuration);
        ConfigureJsonSerializer(configuration);
        ConfigureMvc(context.Services, configuration);
        ConfigureCors(context.Services, configuration);
        ConfigureDistributedLocking(context.Services, configuration);
        ConfigureSecurity(context.Services, configuration, hostingEnvironment.IsDevelopment());
    }
}
