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
using LCH.Abp.Identity.EntityFrameworkCore;
using LCH.Abp.Identity.Session.AspNetCore;
using LCH.Abp.Localization.CultureMap;
using LCH.Abp.LocalizationManagement.EntityFrameworkCore;
using LCH.Abp.Logging.Serilog.Elasticsearch;
using LCH.Abp.OssManagement.SettingManagement;
using LCH.Abp.PermissionManagement;
using LCH.Abp.PermissionManagement.HttpApi;
using LCH.Abp.PermissionManagement.OrganizationUnits;
using LCH.Abp.Saas;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.SettingManagement;
using LCH.Abp.Sms.Platform;
using LCH.Abp.Telemetry.OpenTelemetry;
using LCH.Abp.Telemetry.SkyWalking;
using LCH.Abp.Tencent.SettingManagement;
using LCH.Abp.TextTemplating;
using LCH.Abp.TextTemplating.EntityFrameworkCore;
using LCH.Abp.TextTemplating.Scriban;
using LCH.Abp.WxPusher.SettingManagement;
using LCH.MicroService.BackendAdmin.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Http.Client;
using Volo.Abp.IdentityServer.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.PermissionManagement.IdentityServer;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Swashbuckle;

namespace LCH.MicroService.BackendAdmin;

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
    typeof(AbpIdentityEntityFrameworkCoreModule),// 用户角色权限需要引用包
    typeof(AbpIdentityServerEntityFrameworkCoreModule), // 客户端权限需要引用包
    typeof(AbpPermissionManagementDomainOrganizationUnitsModule), // 组织机构权限管理
    typeof(AbpSaasEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AbpPermissionManagementDomainIdentityServerModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpLocalizationManagementEntityFrameworkCoreModule),
    typeof(AbpTextTemplatingEntityFrameworkCoreModule),

    // 重写模板引擎支持外部本地化
    typeof(AbpTextTemplatingScribanModule),

    typeof(AbpIdentitySessionAspNetCoreModule),

    typeof(BackendAdminMigrationsEntityFrameworkCoreModule),
    typeof(AbpDataDbMigratorModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpEmailingExceptionHandlingModule),
    typeof(AbpTelemetryOpenTelemetryModule),
    typeof(AbpTelemetrySkyWalkingModule),
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
public partial class BackendAdminHttpApiHostModule : AbpModule
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
        ConfigureExceptionHandling();
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
        ConfigureSeedWorker(context.Services, hostingEnvironment.IsDevelopment());
        ConfigureSecurity(context.Services, configuration, hostingEnvironment.IsDevelopment());
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseForwardedHeaders();
        // 本地化
        app.UseMapRequestLocalization();
        // http调用链
        app.UseCorrelationId();
        // 文件系统
        app.MapAbpStaticAssets();
        // 路由
        app.UseRouting();
        // 跨域
        app.UseCors();
        // 认证
        app.UseAuthentication();
        app.UseJwtTokenMiddleware();
        // 多租户
        app.UseMultiTenancy();
        // 会话
        app.UseAbpSession();
        // jwt
        app.UseDynamicClaims();
        // 授权
        app.UseAuthorization();
        // Swagger
        app.UseSwagger();
        // Swagger可视化界面
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Support Admin Service API");

            var configuration = context.GetConfiguration();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes(configuration["AuthServer:Audience"]);
        });
        // 审计日志
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        // 路由
        app.UseConfiguredEndpoints();
    }
}
