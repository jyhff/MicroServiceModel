using LCH.Abp.AspNetCore.HttpOverrides;
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
using LCH.Abp.OssManagement.Minio;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using LCH.Abp.Telemetry.OpenTelemetry;
using LCH.Abp.Telemetry.SkyWalking;
using LCH.Abp.Video;
using LCH.Abp.Video.EntityFrameworkCore;
using LCH.MicroService.VideoService.BackgroundWorkers;
using LCH.MicroService.VideoService.Settings;
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
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Threading;

namespace LCH.MicroService.VideoService;

[DependsOn(
    typeof(AbpSerilogEnrichersApplicationModule),
    typeof(AbpSerilogEnrichersUniqueIdModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAuditLoggingElasticsearchModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpVideoApplicationModule),
    typeof(AbpVideoHttpApiModule),
    typeof(AbpVideoEntityFrameworkCoreModule),
    typeof(AbpHttpClientIdentityModelWebModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpDataDbMigratorModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpAuthorizationOrganizationUnitsModule),
    typeof(AbpCAPEventBusModule),
    typeof(AbpFeaturesValidationRedisModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpLocalizationCultureMapModule),
    typeof(AbpIdentitySessionAspNetCoreModule),
    typeof(AbpTelemetryOpenTelemetryModule),
    typeof(AbpTelemetrySkyWalkingModule),
    typeof(AbpHttpClientModule),
    typeof(AbpOssManagementMinioModule),
    typeof(AbpAspNetCoreMvcWrapperModule),
    typeof(AbpClaimsMappingModule),
    typeof(AbpAspNetCoreHttpOverridesModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAutofacModule)
    )]
public partial class VideoServiceHttpApiHostModule : AbpModule
{
    public static string ApplicationName { get; set; } = "VideoService";

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        PreConfigureWrapper();
        PreForwardedHeaders();
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
        ConfigureVideoSettings(configuration);
        ConfigureMinIO(context.Services, configuration);
        ConfigureBackgroundWorkers(context.Services, configuration);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpRequestLocalization();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "VideoService API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();

        context.ServiceProvider
            .GetRequiredService<IBackgroundWorkerManager>()
            .AddAsync<TranscodingWorker>();
        context.ServiceProvider
            .GetRequiredService<IBackgroundWorkerManager>()
            .AddAsync<UploadSessionCleanupWorker>();
    }

    private void PreConfigureWrapper()
    {
        PreConfigure<AbpAspNetCoreMvcWrapperOptions>(options =>
        {
            options.WrapOnSuccess = true;
            options.WrapOnError = true;
        });
    }

    private void PreForwardedHeaders()
    {
        PreConfigure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
    }

    private void PreConfigureApp(IConfiguration configuration)
    {
        PreConfigure<AbpHttpClientOptions>(options =>
        {
            options.RemoteServices.Default = new RemoteServiceConfiguration
            {
                BaseUrl = configuration["App:RemoteServices:Default:BaseUrl"]
            };
        });
    }

    private void PreConfigureCAP(IConfiguration configuration)
    {
        PreConfigure<CapOptions>(options =>
        {
            options.UseMySql(configuration["CAP:MySql:ConnectionString"]);
            options.UseRabbitMQ(configuration["CAP:RabbitMQ:HostName"],
                configuration.GetValue<int>("CAP:RabbitMQ:Port"),
                configuration["CAP:RabbitMQ:UserName"],
                configuration["CAP:RabbitMQ:Password"]);
            options.UseDashboard();
        });
    }

    private void ConfigureWrapper()
    {
        Configure<AbpAspNetCoreMvcWrapperOptions>(options =>
        {
            options.AutoWrap = true;
        });
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
        });
    }

    private void ConfigureKestrelServer()
    {
        Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 1024 * 1024 * 100;
        });
    }

    private void ConfigureExceptionHandling()
    {
        Configure<AbpExceptionHandlingOptions>(options =>
        {
            options.SendExceptionsDetailsToClients = true;
        });
    }

    private void ConfigureVirtualFileSystem()
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.ReplaceEmbeddedByPhysical<AbpVideoDomainSharedModule>(
                Path.Combine(Directory.GetCurrentDirectory(), "modules/video/LCH.Abp.Video.Domain.Shared"));
        });
    }

    private void ConfigureFeatureManagement()
    {
        Configure<FeatureManagementOptions>(options =>
        {
        });
    }

    private void ConfigureTiming(IConfiguration configuration)
    {
        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Local;
        });
    }

    private void ConfigureCaching(IConfiguration configuration)
    {
        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "VideoService:";
        });
    }

    private void ConfigureAuditing(IConfiguration configuration)
    {
        Configure<AbpAuditingOptions>(options =>
        {
            options.ApplicationName = ApplicationName;
            options.IsEnabled = true;
        });
    }

    private void ConfigureMultiTenancy(IConfiguration configuration)
    {
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = configuration.GetValue<bool>("MultiTenancy:IsEnabled");
        });
    }

    private void ConfigureJsonSerializer(IConfiguration configuration)
    {
        Configure<AbpJsonOptions>(options =>
        {
            options.OutputDateTimeFormat = configuration["Json:OutputDateTimeFormat"];
        });
    }

    private void ConfigureMvc(IServiceCollection services, IConfiguration configuration)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AbpVideoApplicationModule).Assembly);
        });
    }

    private void ConfigureCors(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureSwagger(IServiceCollection services, IConfiguration configuration)
    {
        Configure<AbpSwaggerOptions>(options =>
        {
            options.DocumentTitle = "VideoService API";
            options.DocumentName = "v1";
        });
    }

    private void ConfigureDistributedLocking(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
            return new RedisDistributedLockProvider(redis);
        });
    }

    private void ConfigureSecurity(IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        if (!isDevelopment)
        {
            Configure<AbpAntiForgeryOptions>(options =>
            {
                options.AutoValidate = true;
            });
        }
    }

    private void ConfigureVideoSettings(IConfiguration configuration)
    {
        Configure<VideoSettings>(configuration.GetSection("Video"));
    }

    private void ConfigureMinIO(IServiceCollection services, IConfiguration configuration)
    {
        Configure<AbpOssManagementOptions>(options =>
        {
            options.AddStaticBucket("video-original");
            options.AddStaticBucket("video-transcoded");
            options.AddStaticBucket("video-hls");
            options.AddStaticBucket("video-temp");
            options.AddStaticBucket("video-cover");
        });
    }

    private void ConfigureBackgroundWorkers(IServiceCollection services, IConfiguration configuration)
    {
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = configuration.GetValue<bool>("BackgroundWorkers:IsEnabled", true);
        });
    }
}