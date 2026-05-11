using LCH.Abp.Aliyun.SettingManagement;
using LCH.Abp.AspNetCore.HttpOverrides;
using LCH.Abp.AspNetCore.Mvc.Localization;
using LCH.Abp.AspNetCore.Mvc.Wrapper;
using LCH.Abp.Auditing;
using LCH.Abp.AuditLogging.Elasticsearch;
using LCH.Abp.CachingManagement;
using LCH.Abp.CachingManagement.StackExchangeRedis;
using LCH.Abp.Claims.Mapping;
using LCH.Abp.Data.DbMigrator;
using LCH.Abp.DataProtectionManagement;
using LCH.Abp.Emailing.Platform;
using LCH.Abp.EventBus.CAP;
using LCH.Abp.ExceptionHandling.Emailing;
using LCH.Abp.FeatureManagement;
using LCH.Abp.FeatureManagement.HttpApi;
using LCH.Abp.Identity.Session.AspNetCore;
using LCH.Abp.Localization.CultureMap;
using LCH.Abp.Logging.Serilog.Elasticsearch;
using LCH.Abp.OssManagement.SettingManagement;
using LCH.Abp.PermissionManagement;
using LCH.Abp.PermissionManagement.HttpApi;
using LCH.Abp.PermissionManagement.OrganizationUnits;
using LCH.Abp.Saas;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.SettingManagement;
using LCH.Abp.Sms.Platform;
using LCH.Abp.Tencent.SettingManagement;
using LCH.Abp.TextTemplating;
using LCH.Abp.TextTemplating.Scriban;
using LCH.Abp.WxPusher.SettingManagement;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.PermissionManagement.OpenIddict;
using Volo.Abp.Swashbuckle;

namespace LCH.Abp.MicroService.AdminService;

[DependsOn(
    typeof(AbpCAPEventBusModule),
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpLoggingSerilogElasticsearchModule),
    typeof(AbpAuditLoggingElasticsearchModule),
    typeof(AbpAspNetCoreMvcUiMultiTenancyModule),
    typeof(AbpAspNetCoreMvcLocalizationModule),

    // 设置管理
    typeof(AbpAliyunSettingManagementModule),
    typeof(AbpTencentCloudSettingManagementModule),
    // typeof(AbpWeChatSettingManagementModule),
    typeof(AbpWxPusherSettingManagementModule),
    typeof(AbpOssManagementSettingManagementModule),

    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpDataProtectionManagementApplicationModule),
    typeof(AbpDataProtectionManagementHttpApiModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpFeatureManagementClientModule),
    typeof(AbpAuditingApplicationModule),
    typeof(AbpAuditingHttpApiModule),
    typeof(AbpSaasApplicationModule),
    typeof(AbpSaasHttpApiModule),
    typeof(AbpSaasDbCheckerModule),
    typeof(AbpTextTemplatingApplicationModule),
    typeof(AbpTextTemplatingHttpApiModule),
    typeof(AbpCachingManagementApplicationModule),
    typeof(AbpCachingManagementHttpApiModule),
    typeof(AbpCachingManagementStackExchangeRedisModule),
    typeof(AbpPermissionManagementDomainOrganizationUnitsModule), // 组织机构权限管理
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AbpPermissionManagementDomainOpenIddictModule),

    // 重写模板引擎支持外部本地化
    typeof(AbpTextTemplatingScribanModule),

    typeof(AbpIdentitySessionAspNetCoreModule),

    typeof(AdminServiceMigrationsEntityFrameworkCoreModule),
    typeof(AbpDataDbMigratorModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpEmailingExceptionHandlingModule),
    typeof(AbpHttpClientModule),
    typeof(AbpSmsPlatformModule),
    typeof(AbpEmailingPlatformModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpLocalizationCultureMapModule),
    typeof(AbpAspNetCoreMvcWrapperModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpClaimsMappingModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAutofacModule)
    )]
public partial class AdminServiceModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        PreConfigureWrapper();
        PreConfigureFeature();
        PreConfigureApp(configuration);
        PreConfigureCAP(configuration);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureWrapper();
        ConfigureLocalization();
        ConfigureVirtualFileSystem();
        ConfigureTextTemplating();
        ConfigureSettingManagement();
        ConfigureFeatureManagement();
        ConfigurePermissionManagement();
        ConfigureDataProtectedManagement();
        ConfigureIdentity(configuration);
        ConfigureTiming(configuration);
        ConfigureCaching(configuration);
        ConfigureAuditing(configuration);
        ConfigureMultiTenancy(configuration);
        ConfigureJsonSerializer(configuration);
        ConfigureMvc(context.Services, configuration);
        ConfigureCors(context.Services, configuration);
        ConfigureSwagger(context.Services, configuration);
        ConfigureDistributedLocking(context.Services, configuration);
        ConfigureSecurity(context.Services, configuration, hostingEnvironment.IsDevelopment());
    }
}
