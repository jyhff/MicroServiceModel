using LCH.Abp.Account;
using LCH.Abp.AspNetCore.HttpOverrides;
using LCH.Abp.AspNetCore.Mvc.Localization;
using LCH.Abp.AspNetCore.Mvc.Wrapper;
using LCH.Abp.AuditLogging.Elasticsearch;
using LCH.Abp.Authorization.OrganizationUnits;
using LCH.Abp.BlobStoring.OssManagement;
using LCH.Abp.Claims.Mapping;
using LCH.Abp.Emailing.Platform;
using LCH.Abp.EventBus.CAP;
using LCH.Abp.ExceptionHandling.Emailing;
using LCH.Abp.Exporter.MiniExcel;
using LCH.Abp.Gdpr;
using LCH.Abp.Gdpr.EntityFrameworkCore;
using LCH.Abp.Gdpr.Identity;
using LCH.Abp.Identity;
using LCH.Abp.Identity.EntityFrameworkCore;
using LCH.Abp.Identity.Session.AspNetCore;
using LCH.Abp.Localization.CultureMap;
using LCH.Abp.LocalizationManagement.EntityFrameworkCore;
using LCH.Abp.OpenIddict;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.Sms.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Swashbuckle;

namespace LCH.Abp.MicroService.IdentityService;

[DependsOn(
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreMvcLocalizationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictApplicationModule),
    typeof(AbpOpenIddictHttpApiModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpGdprApplicationModule),
    typeof(AbpGdprHttpApiModule),
    typeof(AbpGdprDomainIdentityModule),
    typeof(AbpGdprEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpSaasEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpLocalizationManagementEntityFrameworkCoreModule),
    typeof(AbpAuthorizationOrganizationUnitsModule),
    typeof(AbpAuditLoggingElasticsearchModule),
    typeof(AbpEmailingExceptionHandlingModule),
    typeof(AbpBlobStoringOssManagementModule),
    typeof(AbpCAPEventBusModule),
    typeof(AbpHttpClientModule),
    typeof(AbpSmsPlatformModule),
    typeof(AbpEmailingPlatformModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpLocalizationCultureMapModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpIdentitySessionAspNetCoreModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpAspNetCoreMvcWrapperModule),
    typeof(AbpExporterMiniExcelModule),
    typeof(AbpClaimsMappingModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAutofacModule)
    )]
public partial class IdentityServiceModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var configuration = context.Services.GetConfiguration();

        PreConfigureWrapper();
        PreConfigureFeature();
        PreForwardedHeaders();
        PreConfigureApp(configuration);
        PreConfigureCAP(configuration);
        PreConfigureIdentity();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureWrapper();
        ConfigureIdentity();
        ConfigureDbContext();
        ConfigureLocalization();
        ConfigureVirtualFileSystem();
        ConfigureFeatureManagement();
        ConfigurePermissionManagement();
        ConfigureBlobStoring(configuration);
        ConfigureUrls(configuration);
        ConfigureCaching(configuration);
        ConfigureTiming(configuration);
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
