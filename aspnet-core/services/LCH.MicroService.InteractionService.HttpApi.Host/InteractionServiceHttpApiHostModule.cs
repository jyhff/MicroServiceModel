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
using LCH.Abp.Notifications;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Bilibili.Interaction;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
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
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Threading;

namespace LCH.MicroService.InteractionService;

[DependsOn(
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAuditLoggingElasticsearchModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreMvcLocalizationModule),
    typeof(AbpNotificationsModule),
    typeof(InteractionDomainModule),
    typeof(InteractionApplicationModule),
    typeof(InteractionHttpApiModule),
    typeof(InteractionEntityFrameworkCoreModule),
    typeof(AbpIdentityHttpApiClientModule),
    typeof(AbpHttpClientIdentityModelWebModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpSaasEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpLocalizationManagementEntityFrameworkCoreModule),
    typeof(AbpDataDbMigratorModule),
    typeof(AbpAspNetCoreAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpClaimsMappingModule),
    typeof(AbpNotificationsModule),
    typeof(AbpIdentitySessionAspNetCoreModule),
    typeof(AbpExceptionHandlingEmailingModule),
    typeof(AbpFeaturesLimitValidationRedisModule),
    typeof(AbpLocalizationCultureMapModule),
    typeof(AbpEventBusCAPModule))]
public partial class InteractionServiceHttpApiHostModule : AbpModule
{
    public static string ApplicationName { get; set; } = "InteractionService";
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigureFeature();
        PreForwardedHeaders();
        PreConfigureApp(context.Services.GetConfiguration());
    }

    private void PreConfigureFeature()
    {
        OneTimeRunner.Run(() =>
        {
            GlobalFeatureManager.Instance.Modules.Editions().EnableAll();
        });
    }

    private void PreForwardedHeaders()
    {
    }

    private void PreConfigureApp(IConfiguration configuration)
    {
        AbpSerilogEnrichersConsts.ApplicationName = ApplicationName;
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureSwagger(context, configuration);
        ConfigureJwtAuthentication(context, configuration);
        ConfigureRedis(context, configuration, hostingEnvironment);
        ConfigureCors(context, configuration, hostingEnvironment);
        ConfigureCAP(context, configuration);
        ConfigureDistributedLock(context, configuration, hostingEnvironment);
        ConfigureExceptionHandling(context, configuration, hostingEnvironment);
        ConfigureLocalization(context, configuration, hostingEnvironment);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpClaimsMap();
        app.UseMultiTenancy();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "InteractionService API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }

    public override async Task OnApplicationShutdownAsync(ApplicationShutdownContext context)
    {
    }
}