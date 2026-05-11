using LCH.Abp.AspNetCore.HttpOverrides;
using LCH.Abp.AspNetCore.Mvc.Localization;
using LCH.Abp.AspNetCore.Mvc.Wrapper;
using LCH.Abp.AuditLogging.Elasticsearch;
using LCH.Abp.Authorization.OrganizationUnits;
using LCH.Abp.Claims.Mapping;
using LCH.Abp.Data.DbMigrator;
using LCH.Abp.EventBus.CAP;
using LCH.Abp.ExceptionHandling.Emailing;
using LCH.Abp.Features.LimitValidation.Redis;
using LCH.Abp.Identity.Session.AspNetCore;
using LCH.Abp.Localization.CultureMap;
using LCH.Abp.LocalizationManagement.EntityFrameworkCore;
using LCH.Abp.MicroService.PlatformService.BackgroundWorkers;
using LCH.Abp.Notifications;
using LCH.Abp.OssManagement;
using LCH.Abp.OssManagement.Aliyun;
using LCH.Abp.OssManagement.FileSystem;
using LCH.Abp.OssManagement.Imaging;
using LCH.Abp.OssManagement.Minio;
using LCH.Abp.OssManagement.Nexus;
using LCH.Abp.OssManagement.SettingManagement;
using LCH.Abp.OssManagement.Tencent;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.Sms.Aliyun;
using LCH.Platform;
using LCH.Platform.EntityFrameworkCore;
using LCH.Platform.HttpApi;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Http.Client;
using Volo.Abp.Http.Client.IdentityModel.Web;
using Volo.Abp.Identity;
using Volo.Abp.Imaging;
using Volo.Abp.MailKit;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Threading;

namespace LCH.Abp.MicroService.PlatformService;

[DependsOn(
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAuditLoggingElasticsearchModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreMvcLocalizationModule),
    typeof(AbpOssManagementAliyunModule),    // 阿里云存储提供者模块
    typeof(AbpOssManagementTencentModule),   // 腾讯云存储提供者模块
    typeof(AbpOssManagementNexusModule),     // Nexus存储提供者模块
    typeof(AbpOssManagementMinioModule),     // Minio存储提供者模块
    typeof(AbpOssManagementFileSystemModule),// 本地文件系统提供者模块
    typeof(AbpOssManagementImagingModule), // 对象存储图形处理模块
    typeof(AbpOssManagementApplicationModule),
    typeof(AbpOssManagementHttpApiModule),
    typeof(AbpOssManagementSettingManagementModule),
    typeof(AbpImagingImageSharpModule),
    typeof(PlatformApplicationModule),
    typeof(PlatformHttpApiModule),
    typeof(PlatformEntityFrameworkCoreModule),
    typeof(AbpIdentityHttpApiClientModule),
    typeof(AbpHttpClientIdentityModelWebModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpSaasEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpLocalizationManagementEntityFrameworkCoreModule),
    typeof(PlatformServiceMigrationsEntityFrameworkCoreModule),
    typeof(AbpDataDbMigratorModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpAuthorizationOrganizationUnitsModule),
    typeof(AbpNotificationsModule),
    typeof(AbpEmailingExceptionHandlingModule),
    typeof(AbpCAPEventBusModule),
    typeof(AbpFeaturesValidationRedisModule),
    // typeof(AbpFeaturesClientModule),// 当需要客户端特性限制时取消注释此模块
    // typeof(AbpFeaturesValidationRedisClientModule),// 当需要客户端特性限制时取消注释此模块
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpLocalizationCultureMapModule),
    typeof(AbpIdentitySessionAspNetCoreModule),
    typeof(AbpHttpClientModule),
    typeof(AbpMailKitModule),
    typeof(AbpAliyunSmsModule),
    typeof(AbpAspNetCoreMvcWrapperModule),
    typeof(AbpClaimsMappingModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAutofacModule)
    )]
public partial class PlatformServiceModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        PreConfigureWrapper();
        PreForwardedHeaders();
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
        ConfigureKestrelServer();
        ConfigureExceptionHandling();
        ConfigureVirtualFileSystem();
        ConfigureFeatureManagement();
        ConfigurePermissionManagement();
        ConfigureTiming(configuration);
        ConfigureCaching(configuration);
        ConfigureIdentity(configuration);
        ConfigureAuditing(configuration);
        ConfigureMultiTenancy(configuration);
        ConfigureJsonSerializer(configuration);
        ConfigureMvc(context.Services, configuration);
        ConfigureCors(context.Services, configuration);
        ConfigureSwagger(context.Services, configuration);
        ConfigureDistributedLocking(context.Services, configuration);
        ConfigureSecurity(context.Services, configuration, hostingEnvironment.IsDevelopment());

        ConfigurePlatformModule(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(() => OnApplicationInitializationAsync(context));
    }

    public async override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<AbpOssManagementOptions>>().Value;
        if (options.IsCleanupEnabled)
        {
            await context.ServiceProvider
                .GetRequiredService<IBackgroundWorkerManager>()
                .AddAsync(context.ServiceProvider.GetRequiredService<OssObjectTempCleanupBackgroundWorker>());
        }
    }
}
