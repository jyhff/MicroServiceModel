using LCH.Abp.AspNetCore.HttpOverrides;
using LCH.Abp.AspNetCore.Mvc.Localization;
using LCH.Abp.AspNetCore.Mvc.Wrapper;
using LCH.Abp.AuditLogging.Elasticsearch;
using LCH.Abp.Authorization.OrganizationUnits;
using LCH.Abp.BackgroundTasks.DistributedLocking;
using LCH.Abp.BackgroundTasks.ExceptionHandling;
using LCH.Abp.BackgroundTasks.Quartz;
using LCH.Abp.Claims.Mapping;
using LCH.Abp.Data.DbMigrator;
using LCH.Abp.Emailing.Platform;
using LCH.Abp.EventBus.CAP;
using LCH.Abp.ExceptionHandling.Notifications;
using LCH.Abp.Features.LimitValidation.Redis;
using LCH.Abp.Identity.EntityFrameworkCore;
using LCH.Abp.Identity.Notifications;
using LCH.Abp.Identity.Session.AspNetCore;
using LCH.Abp.Identity.WeChat;
using LCH.Abp.Identity.WeChat.Work;
using LCH.Abp.IM.SignalR;
using LCH.Abp.Localization.CultureMap;
using LCH.Abp.LocalizationManagement.EntityFrameworkCore;
using LCH.Abp.MessageService;
using LCH.Abp.MessageService.EntityFrameworkCore;
using LCH.Abp.Notifications;
using LCH.Abp.Notifications.Common;
using LCH.Abp.Notifications.Emailing;
using LCH.Abp.Notifications.EntityFrameworkCore;
using LCH.Abp.Notifications.Jobs;
using LCH.Abp.Notifications.SignalR;
using LCH.Abp.Notifications.Sms;
using LCH.Abp.Notifications.Templating;
using LCH.Abp.Notifications.WeChat.MiniProgram;
using LCH.Abp.Notifications.WeChat.Work;
using LCH.Abp.Notifications.WxPusher;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.Sms.Platform;
using LCH.Abp.TaskManagement.EntityFrameworkCore;
using LCH.Abp.TextTemplating.EntityFrameworkCore;
using LCH.Abp.TextTemplating.Scriban;
using LCH.Abp.WeChat.Official.Handlers;
using LCH.Abp.WeChat.Work.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Swashbuckle;

namespace LCH.Abp.MicroService.MessageService;

[DependsOn(
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAuditLoggingElasticsearchModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreMvcLocalizationModule),
    typeof(AbpMessageServiceApplicationModule),
    typeof(AbpMessageServiceHttpApiModule),
    typeof(AbpNotificationsApplicationModule),
    typeof(AbpNotificationsHttpApiModule),
    typeof(AbpIdentityWeChatModule),
    typeof(AbpIdentityWeChatWorkModule),
    typeof(AbpBackgroundTasksQuartzModule),
    typeof(AbpBackgroundTasksDistributedLockingModule),
    typeof(AbpBackgroundTasksExceptionHandlingModule),
    typeof(TaskManagementEntityFrameworkCoreModule),
    typeof(AbpMessageServiceEntityFrameworkCoreModule),
    typeof(AbpNotificationsEntityFrameworkCoreModule),
    typeof(AbpSaasEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpLocalizationManagementEntityFrameworkCoreModule),
    typeof(AbpTextTemplatingEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(MessageServiceMigrationsEntityFrameworkCoreModule),
    typeof(AbpDataDbMigratorModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpAuthorizationOrganizationUnitsModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpIMSignalRModule),
    typeof(AbpNotificationsJobsModule),
    typeof(AbpNotificationsCommonModule),
    typeof(AbpNotificationsSmsModule),
    typeof(AbpNotificationsEmailingModule),
    typeof(AbpNotificationsSignalRModule),
    typeof(AbpNotificationsWxPusherModule),
    typeof(AbpNotificationsWeChatMiniProgramModule),
    typeof(AbpNotificationsWeChatWorkModule),
    typeof(AbpNotificationsExceptionHandlingModule),
    typeof(AbpNotificationsTemplatingModule),
    typeof(AbpWeChatWorkHandlersModule),
    typeof(AbpWeChatOfficialHandlersModule),
    typeof(AbpIdentityNotificationsModule),

    // 重写模板引擎支持外部本地化
    typeof(AbpTextTemplatingScribanModule),

    typeof(AbpCAPEventBusModule),
    typeof(AbpFeaturesValidationRedisModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpLocalizationCultureMapModule),
    typeof(AbpIdentitySessionAspNetCoreModule),
    typeof(AbpEmailingPlatformModule),
    typeof(AbpSmsPlatformModule),
    typeof(AbpHttpClientModule),
    typeof(AbpClaimsMappingModule),
    typeof(AbpAspNetCoreMvcWrapperModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAutofacModule)
    )]
public partial class MessageServiceModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        PreConfigureWrapper();
        PreConfigureFeature();
        PreForwardedHeaders();
        PreConfigureApp(configuration);
        PreConfigureCAP(configuration);
        PreConfigureQuartz(configuration);
        PreConfigureSignalR(configuration);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureWrapper();
        ConfigureLocalization();
        ConfigureNotifications();
        ConfigureTextTemplating();
        ConfigureVirtualFileSystem();
        ConfigureFeatureManagement();
        ConfigurePermissionManagement();
        ConfigureTiming(configuration);
        ConfigureCaching(configuration);
        ConfigureAuditing(configuration);
        ConfigureIdentity(configuration);
        ConfigureMultiTenancy(configuration);
        ConfigureJsonSerializer(configuration);
        ConfigureMvc(context.Services, configuration);
        ConfigureCors(context.Services, configuration);
        ConfigureSwagger(context.Services, configuration);
        ConfigureBackgroundTasks(context.Services, configuration);
        ConfigureDistributedLocking(context.Services, configuration);
        ConfigureSecurity(context.Services, configuration, hostingEnvironment.IsDevelopment());
    }
}
