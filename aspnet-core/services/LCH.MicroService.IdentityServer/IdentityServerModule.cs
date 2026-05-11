using LCH.Abp.Account;
using LCH.Abp.Account.Web.IdentityServer;
using LCH.Abp.AspNetCore.HttpOverrides;
using LCH.Abp.AspNetCore.MultiTenancy;
using LCH.Abp.AspNetCore.Mvc.Wrapper;
using LCH.Abp.AuditLogging.Elasticsearch;
using LCH.Abp.Authentication.QQ;
using LCH.Abp.Authentication.WeChat;
using LCH.Abp.Data.DbMigrator;
using LCH.Abp.Emailing.Platform;
using LCH.Abp.EventBus.CAP;
using LCH.Abp.Exporter.MiniExcel;
using LCH.Abp.Gdpr;
using LCH.Abp.Gdpr.Web;
using LCH.Abp.Identity.AspNetCore.Session;
using LCH.Abp.Identity.OrganizaztionUnits;
using LCH.Abp.Identity.Session.AspNetCore;
using LCH.Abp.IdentityServer;
using LCH.Abp.IdentityServer.LinkUser;
using LCH.Abp.IdentityServer.Portal;
using LCH.Abp.IdentityServer.Session;
using LCH.Abp.IdentityServer.WeChat.Work;
using LCH.Abp.Localization.CultureMap;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.Sms.Platform;
using LCH.Abp.Telemetry.OpenTelemetry;
using LCH.Abp.Telemetry.SkyWalking;
using LCH.MicroService.IdentityServer.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.Identity;

namespace LCH.MicroService.IdentityServer;

[DependsOn(
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpAccountWebIdentityServerModule),
    typeof(AbpGdprApplicationModule),
    typeof(AbpGdprHttpApiModule),
    typeof(AbpGdprWebModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpIdentityAspNetCoreSessionModule),
    typeof(AbpIdentityServerSessionModule),
    typeof(AbpIdentitySessionAspNetCoreModule),
    typeof(AbpIdentityServerSmsModule),
    typeof(AbpIdentityServerLinkUserModule),
    typeof(AbpIdentityServerPortalModule),
    typeof(AbpIdentityServerWeChatWorkModule),
    typeof(AbpAuthenticationQQModule),
    typeof(AbpAuthenticationWeChatModule),
    typeof(AbpIdentityOrganizaztionUnitsModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(IdentityServerMigrationsEntityFrameworkCoreModule),
    typeof(AbpDataDbMigratorModule),
    typeof(AbpAuditLoggingElasticsearchModule), // 放在 AbpIdentity 模块之后,避免被覆盖
    typeof(AbpLocalizationCultureMapModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreMvcWrapperModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpTelemetryOpenTelemetryModule),
    typeof(AbpTelemetrySkyWalkingModule),
    typeof(AbpExporterMiniExcelModule),
    typeof(AbpEmailingPlatformModule),
    typeof(AbpSmsPlatformModule),
    typeof(AbpCAPEventBusModule)
    )]
public partial class IdentityServerModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        PreConfigureFeature();
        PreForwardedHeaders();
        PreConfigureApp(configuration);
        PreConfigureCAP(configuration);
        PreConfigureCertificate(configuration, hostingEnvironment);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureCaching(configuration);
        ConfigureIdentity(configuration);
        ConfigureFeatureManagement();
        ConfigureSettingManagement();
        ConfigureLocalization();
        ConfigureAuditing(configuration);
        ConfigureDataSeeder();
        ConfigureMvcUiTheme();
        ConfigureUrls(configuration);
        ConfigureMultiTenancy(configuration);
        ConfigureJsonSerializer(configuration);
        ConfigureMvc(context.Services, configuration);
        ConfigureCors(context.Services, configuration);
        ConfigureDistributedLocking(context.Services, configuration);
        ConfigureSeedWorker(context.Services, hostingEnvironment.IsDevelopment());
        ConfigureSecurity(context.Services, configuration, hostingEnvironment.IsDevelopment());
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();
        app.UseMapRequestLocalization();
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseErrorPage();
            app.UseHsts();
        }

        // app.UseHttpsRedirection();
        app.UseCookiePolicy();
        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseJwtTokenMiddleware();
        app.UseMultiTenancy();
        app.UseAbpSession();
        app.UseDynamicClaims();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
