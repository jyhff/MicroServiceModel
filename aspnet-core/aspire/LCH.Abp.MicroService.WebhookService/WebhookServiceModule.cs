using LCH.Abp.AspNetCore.HttpOverrides;
using LCH.Abp.AspNetCore.Mvc.Localization;
using LCH.Abp.AspNetCore.Mvc.Wrapper;
using LCH.Abp.AuditLogging.Elasticsearch;
using LCH.Abp.Authorization.OrganizationUnits;
using LCH.Abp.BackgroundTasks.DistributedLocking;
using LCH.Abp.BackgroundTasks.ExceptionHandling;
using LCH.Abp.BackgroundTasks.Quartz;
using LCH.Abp.Claims.Mapping;
using LCH.Abp.Dapr.Client.Wrapper;
using LCH.Abp.Emailing.Platform;
using LCH.Abp.EventBus.CAP;
using LCH.Abp.ExceptionHandling.Emailing;
using LCH.Abp.Http.Client.Wrapper;
using LCH.Abp.Identity.Session.AspNetCore;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.Sms.Platform;
using LCH.Abp.Webhooks.EventBus;
using LCH.Abp.Webhooks.Identity;
using LCH.Abp.Webhooks.Saas;
using LCH.Abp.WebhooksManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Http.Client.IdentityModel.Web;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace LCH.Abp.MicroService.WebhookService;

[DependsOn(
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpAuditLoggingElasticsearchModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(WebhooksManagementApplicationModule),
    typeof(WebhooksManagementHttpApiModule),
    typeof(AbpWebhooksIdentityModule),
    typeof(AbpWebhooksSaasModule),
    typeof(AbpWebhooksEventBusModule),
    typeof(AbpBackgroundTasksQuartzModule),
    typeof(AbpBackgroundTasksDistributedLockingModule),
    typeof(AbpBackgroundTasksExceptionHandlingModule),
    typeof(WebhookServiceMigrationsEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpAuthorizationOrganizationUnitsModule),
    typeof(AbpEmailingExceptionHandlingModule),
    typeof(AbpCAPEventBusModule),
    typeof(AbpHttpClientIdentityModelWebModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreMvcLocalizationModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpDistributedLockingModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpHttpClientWrapperModule),
    typeof(AbpDaprClientWrapperModule),
    typeof(AbpClaimsMappingModule),
    typeof(AbpEmailingPlatformModule),
    typeof(AbpSmsPlatformModule),
    typeof(AbpAspNetCoreMvcWrapperModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpIdentitySessionAspNetCoreModule),
    typeof(AbpAutofacModule)
    )]
public partial class WebhookServiceModule : AbpModule
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
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureWrapper();
        ConfigureLocalization();
        ConfigureVirtualFileSystem();
        ConfigureFeatureManagement();
        ConfigureSettingManagement();
        ConfigurePermissionManagement();
        ConfigureTiming(configuration);
        ConfigureCaching(configuration);
        ConfigureAuditing(configuration);
        ConfigureIdentity(configuration);
        ConfigureMultiTenancy(configuration);
        ConfigureWebhooks(context.Services);
        ConfigureJsonSerializer(configuration);
        ConfigureMvc(context.Services, configuration);
        ConfigureCors(context.Services, configuration);
        ConfigureSwagger(context.Services, configuration);
        ConfigureDistributedLock(context.Services, configuration);
        ConfigureBackgroundTasks(context.Services, configuration);
        ConfigureSecurity(context.Services, configuration, hostingEnvironment.IsDevelopment());
    }
}
