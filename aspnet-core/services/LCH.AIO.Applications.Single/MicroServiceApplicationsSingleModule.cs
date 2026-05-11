using LCH.Abp.Account;
//using LCH.Abp.Account.Templates;
using LCH.Abp.Aliyun.SettingManagement;
using LCH.Abp.AspNetCore.HttpOverrides;
using LCH.Abp.AspNetCore.Mvc.Idempotent.Wrapper;
using LCH.Abp.AspNetCore.Mvc.Localization;
using LCH.Abp.AspNetCore.Mvc.Wrapper;
using LCH.Abp.Auditing;
using LCH.Abp.AuditLogging.EntityFrameworkCore;
using LCH.Abp.Authentication.QQ;
using LCH.Abp.Authentication.WeChat;
using LCH.Abp.Authorization.OrganizationUnits;
using LCH.Abp.BackgroundTasks;
using LCH.Abp.BackgroundTasks.Activities;
using LCH.Abp.BackgroundTasks.DistributedLocking;
using LCH.Abp.BackgroundTasks.EventBus;
using LCH.Abp.BackgroundTasks.ExceptionHandling;
using LCH.Abp.BackgroundTasks.Jobs;
using LCH.Abp.BackgroundTasks.Notifications;
using LCH.Abp.BackgroundTasks.Quartz;
using LCH.Abp.CachingManagement;
using LCH.Abp.CachingManagement.StackExchangeRedis;
using LCH.Abp.Dapr.Client;
using LCH.Abp.Data.DbMigrator;
using LCH.Abp.DataProtectionManagement;
using LCH.Abp.DataProtectionManagement.EntityFrameworkCore;
// using LCH.Abp.Demo;
// using LCH.Abp.Demo.EntityFrameworkCore;
using LCH.Abp.ExceptionHandling;
using LCH.Abp.ExceptionHandling.Emailing;
using LCH.Abp.Exporter.MiniExcel;
using LCH.Abp.FeatureManagement;
using LCH.Abp.FeatureManagement.HttpApi;
using LCH.Abp.Features.LimitValidation;
using LCH.Abp.Features.LimitValidation.Redis.Client;
using LCH.Abp.Http.Client.Wrapper;
using LCH.Abp.Identity;
using LCH.Abp.Identity.AspNetCore.Session;
using LCH.Abp.Identity.EntityFrameworkCore;
using LCH.Abp.Identity.Notifications;
using LCH.Abp.Identity.OrganizaztionUnits;
using LCH.Abp.Identity.Session.AspNetCore;
using LCH.Abp.Identity.WeChat;
using LCH.Abp.IdGenerator;
using LCH.Abp.IM.SignalR;
using LCH.Abp.Localization.CultureMap;
//using LCH.Abp.Localization.Persistence;
using LCH.Abp.LocalizationManagement;
using LCH.Abp.LocalizationManagement.EntityFrameworkCore;
using LCH.Abp.MessageService;
using LCH.Abp.MessageService.EntityFrameworkCore;
using LCH.Abp.MultiTenancy.Editions;
using LCH.Abp.Notifications;
using LCH.Abp.Notifications.Common;
using LCH.Abp.Notifications.Emailing;
using LCH.Abp.Notifications.EntityFrameworkCore;
using LCH.Abp.Notifications.SignalR;
using LCH.Abp.Notifications.WeChat.MiniProgram;
using LCH.Abp.OpenApi.Authorization;
using LCH.Abp.OpenIddict;
using LCH.Abp.OpenIddict.AspNetCore;
using LCH.Abp.OpenIddict.AspNetCore.Session;
using LCH.Abp.OpenIddict.Portal;
using LCH.Abp.OpenIddict.Sms;
using LCH.Abp.OpenIddict.WeChat;
using LCH.Abp.OpenIddict.WeChat.Work;
using LCH.Abp.OssManagement;
using LCH.Abp.OssManagement.FileSystem;
// using LCH.Abp.OssManagement.Imaging;
using LCH.Abp.OssManagement.SettingManagement;
using LCH.Abp.PermissionManagement;
using LCH.Abp.PermissionManagement.HttpApi;
using LCH.Abp.PermissionManagement.OrganizationUnits;
using LCH.Abp.Saas;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.SettingManagement;
using LCH.Abp.Sms.Aliyun;
using LCH.Abp.TaskManagement;
using LCH.Abp.TaskManagement.EntityFrameworkCore;
using LCH.Abp.Tencent.QQ;
using LCH.Abp.Tencent.SettingManagement;
using LCH.Abp.TextTemplating;
using LCH.Abp.TextTemplating.EntityFrameworkCore;
using LCH.Abp.UI.Navigation;
using LCH.Abp.UI.Navigation.VueVbenAdmin;
using LCH.Abp.Webhooks;
using LCH.Abp.Webhooks.EventBus;
using LCH.Abp.Webhooks.Identity;
using LCH.Abp.Webhooks.Saas;
using LCH.Abp.WebhooksManagement;
using LCH.Abp.WebhooksManagement.EntityFrameworkCore;
using LCH.Abp.WeChat.MiniProgram;
using LCH.Abp.WeChat.Official;
using LCH.Abp.WeChat.Official.Handlers;
using LCH.Abp.WeChat.SettingManagement;
using LCH.Abp.WeChat.Work;
using LCH.Abp.WeChat.Work.Handlers;
using LCH.Platform;
using LCH.Platform.EntityFrameworkCore;
using LCH.Platform.HttpApi;
using LCH.Platform.Settings.VueVbenAdmin;
using LCH.Platform.Theme.VueVbenAdmin;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.EventBus;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Imaging;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.PermissionManagement.OpenIddict;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Threading;
// using LCH.Abp.Elsa.EntityFrameworkCore.MySql;
using Volo.Abp.EntityFrameworkCore.PostgreSql;

namespace LY.AIO.Applications.Single;

[DependsOn(
    typeof(AbpAccountApplicationModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAuditingApplicationModule),
    typeof(AbpAuditingHttpApiModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpCachingManagementStackExchangeRedisModule),
    typeof(AbpCachingManagementApplicationModule),
    typeof(AbpCachingManagementHttpApiModule),
    typeof(AbpIdentityAspNetCoreSessionModule),
    typeof(AbpIdentitySessionAspNetCoreModule),
    typeof(AbpIdentityNotificationsModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpLocalizationManagementDomainModule),
    typeof(AbpLocalizationManagementApplicationModule),
    typeof(AbpLocalizationManagementHttpApiModule),
    typeof(AbpLocalizationManagementEntityFrameworkCoreModule),
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpMessageServiceDomainModule),
    typeof(AbpMessageServiceApplicationModule),
    typeof(AbpMessageServiceHttpApiModule),
    typeof(AbpMessageServiceEntityFrameworkCoreModule),
    typeof(AbpNotificationsDomainModule),
    typeof(AbpNotificationsApplicationModule),
    typeof(AbpNotificationsHttpApiModule),
    typeof(AbpNotificationsEntityFrameworkCoreModule),

    //typeof(AbpIdentityServerSessionModule),
    //typeof(AbpIdentityServerApplicationModule),
    //typeof(AbpIdentityServerHttpApiModule),
    //typeof(AbpIdentityServerEntityFrameworkCoreModule),

    typeof(AbpOpenIddictAspNetCoreModule),
    typeof(AbpOpenIddictAspNetCoreSessionModule),
    typeof(AbpOpenIddictApplicationModule),
    typeof(AbpOpenIddictHttpApiModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpOpenIddictSmsModule),
    typeof(AbpOpenIddictPortalModule),
    typeof(AbpOpenIddictWeChatModule),
    typeof(AbpOpenIddictWeChatWorkModule),

    //typeof(AbpOssManagementMinioModule), // 取消注释以使用Minio
    typeof(AbpOssManagementFileSystemModule),
    // typeof(AbpOssManagementImagingModule),
    typeof(AbpOssManagementDomainModule),
    typeof(AbpOssManagementApplicationModule),
    typeof(AbpOssManagementHttpApiModule),
    typeof(AbpOssManagementSettingManagementModule),
    typeof(AbpImagingImageSharpModule),

    typeof(PlatformDomainModule),
    typeof(PlatformApplicationModule),
    typeof(PlatformHttpApiModule),
    typeof(PlatformEntityFrameworkCoreModule),
    typeof(PlatformSettingsVueVbenAdminModule),
    typeof(PlatformThemeVueVbenAdminModule),
    typeof(AbpUINavigationVueVbenAdminModule),

    typeof(AbpSaasDomainModule),
    typeof(AbpSaasApplicationModule),
    typeof(AbpSaasHttpApiModule),
    typeof(AbpSaasEntityFrameworkCoreModule),

    typeof(TaskManagementDomainModule),
    typeof(TaskManagementApplicationModule),
    typeof(TaskManagementHttpApiModule),
    typeof(TaskManagementEntityFrameworkCoreModule),

    typeof(AbpTextTemplatingDomainModule),
    typeof(AbpTextTemplatingApplicationModule),
    typeof(AbpTextTemplatingHttpApiModule),
    typeof(AbpTextTemplatingEntityFrameworkCoreModule),

    typeof(AbpWebhooksModule),
    typeof(AbpWebhooksEventBusModule),
    typeof(AbpWebhooksIdentityModule),
    typeof(AbpWebhooksSaasModule),
    typeof(WebhooksManagementDomainModule),
    typeof(WebhooksManagementApplicationModule),
    typeof(WebhooksManagementHttpApiModule),
    typeof(WebhooksManagementEntityFrameworkCoreModule),

    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),

    typeof(AbpSettingManagementDomainModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),

    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AbpPermissionManagementDomainOpenIddictModule),
    // typeof(AbpPermissionManagementDomainIdentityServerModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementDomainOrganizationUnitsModule), // 组织机构权限管理

    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    // typeof(AbpEntityFrameworkCoreMySQLModule),

    typeof(AbpAliyunSmsModule),
    typeof(AbpAliyunSettingManagementModule),

    typeof(AbpAuthenticationQQModule),
    typeof(AbpAuthenticationWeChatModule),
    typeof(AbpAuthorizationOrganizationUnitsModule),
    typeof(AbpIdentityOrganizaztionUnitsModule),

    typeof(AbpBackgroundTasksModule),
    typeof(AbpBackgroundTasksActivitiesModule),
    typeof(AbpBackgroundTasksDistributedLockingModule),
    typeof(AbpBackgroundTasksEventBusModule),
    typeof(AbpBackgroundTasksExceptionHandlingModule),
    typeof(AbpBackgroundTasksJobsModule),
    typeof(AbpBackgroundTasksNotificationsModule),
    typeof(AbpBackgroundTasksQuartzModule),

    typeof(AbpDataProtectionManagementApplicationModule),
    typeof(AbpDataProtectionManagementHttpApiModule),
    typeof(AbpDataProtectionManagementEntityFrameworkCoreModule),

    // typeof(AbpDemoApplicationModule),
    // typeof(AbpDemoHttpApiModule),
    // typeof(AbpDemoEntityFrameworkCoreModule),

    typeof(AbpDaprClientModule),
    typeof(AbpExceptionHandlingModule),
    typeof(AbpEmailingExceptionHandlingModule),
    typeof(AbpFeaturesLimitValidationModule),
    typeof(AbpFeaturesValidationRedisClientModule),
    typeof(AbpAspNetCoreMvcLocalizationModule),

    typeof(AbpLocalizationCultureMapModule),
    //typeof(AbpLocalizationPersistenceModule),

    typeof(AbpOpenApiAuthorizationModule),

    typeof(AbpIMSignalRModule),

    typeof(AbpNotificationsModule),
    typeof(AbpNotificationsCommonModule),
    typeof(AbpNotificationsSignalRModule),
    typeof(AbpNotificationsEmailingModule),
    typeof(AbpMultiTenancyEditionsModule),

    typeof(AbpTencentQQModule),
    typeof(AbpTencentCloudSettingManagementModule),

    typeof(AbpIdentityWeChatModule),
    typeof(AbpNotificationsWeChatMiniProgramModule),
    typeof(AbpWeChatMiniProgramModule),
    typeof(AbpWeChatOfficialModule),
    typeof(AbpWeChatOfficialApplicationModule),
    typeof(AbpWeChatOfficialHttpApiModule),
    typeof(AbpWeChatWorkModule),
    typeof(AbpWeChatWorkApplicationModule),
    typeof(AbpWeChatWorkHttpApiModule),
    typeof(AbpWeChatOfficialHandlersModule),
    typeof(AbpWeChatWorkHandlersModule),
    typeof(AbpWeChatSettingManagementModule),

    typeof(AbpDataDbMigratorModule),
    typeof(AbpIdGeneratorModule),
    typeof(AbpUINavigationModule),
    //typeof(AbpAccountTemplatesModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpCachingStackExchangeRedisModule),
    // typeof(AbpElsaModule),
    // typeof(AbpElsaServerModule),
    // typeof(AbpElsaActivitiesModule),
    // typeof(AbpElsaEntityFrameworkCoreModule),
    // typeof(AbpElsaEntityFrameworkCorePostgreSqlModule),
    // typeof(AbpElsaModule),
    // typeof(AbpElsaServerModule),
    // typeof(AbpElsaActivitiesModule),
    // typeof(AbpElsaEntityFrameworkCoreModule),
    // typeof(AbpElsaEntityFrameworkCoreMySqlModule),

    typeof(AbpExporterMiniExcelModule),
    typeof(AbpAspNetCoreMvcUiMultiTenancyModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpHttpClientWrapperModule),
    typeof(AbpAspNetCoreMvcWrapperModule),
    typeof(AbpAspNetCoreMvcIdempotentWrapperModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpAspNetCoreMvcUiBasicThemeModule),
    typeof(AbpEventBusModule),
    typeof(AbpAutofacModule)
    )]
public partial class MicroServiceApplicationsSingleModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        PreConfigureWrapper();
        PreConfigureFeature();
        PreConfigureIdentity();
        PreConfigureApp(configuration);
        PreConfigureQuartz(configuration);
        PreConfigureAuthServer(configuration);
        PreConfigureElsa(context.Services, configuration);
        PreConfigureCertificate(configuration, hostingEnvironment);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureWeChat();
        ConfigureWrapper();
        ConfigureExporter();
        ConfigureAuditing();
        ConfigureDbContext();
        ConfigureIdempotent();
        ConfigureMvcUiTheme();
        ConfigureDataSeeder();
        ConfigureLocalization();
        ConfigureKestrelServer();
        ConfigureBackgroundTasks();
        ConfigureExceptionHandling();
        ConfigureVirtualFileSystem();
        ConfigureEntityDataProtected();
        ConfigureUrls(configuration);
        ConfigureCaching(configuration);
        ConfigureAuditing(configuration);
        ConfigureIdentity(configuration);
        ConfigureAuthServer(configuration);
        ConfigureSwagger(context.Services);
        ConfigureEndpoints(context.Services);
        ConfigureBlobStoring(configuration);
        ConfigureMultiTenancy(configuration);
        ConfigureJsonSerializer(configuration);
        ConfigureTextTemplating(configuration);
        ConfigureFeatureManagement(configuration);
        ConfigureSettingManagement(configuration);
        ConfigureWebhooksManagement(configuration);
        ConfigurePermissionManagement(configuration);
        ConfigureNotificationManagement(configuration);
        ConfigureCors(context.Services, configuration);
        ConfigureDistributedLock(context.Services, configuration);
        ConfigureSecurity(context.Services, configuration, hostingEnvironment.IsDevelopment());
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(async () => await OnApplicationInitializationAsync(context));
    }

    public async override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.ServiceProvider.GetRequiredService<IDataSeeder>().SeedAsync(); ;
    }
}
