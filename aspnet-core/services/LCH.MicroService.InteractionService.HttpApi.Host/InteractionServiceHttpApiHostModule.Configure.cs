using DotNetCore.CAP;
using LCH.Abp.ExceptionHandling;
using LCH.Abp.Localization.CultureMap;
using LCH.Abp.Serilog.Enrichers.Application;
using LCH.Abp.Serilog.Enrichers.UniqueId;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Threading;
using Volo.Abp.VirtualFileSystem;

namespace LCH.MicroService.InteractionService;

public partial class InteractionServiceHttpApiHostModule
{
    private void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"],
            new Dictionary<string, string>
            {
                {"InteractionService", "InteractionService API"}
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "InteractionService API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    private void ConfigureJwtAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata");
                options.Audience = "InteractionService";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["AuthServer:Authority"],
                    ValidAudiences = new[] { "InteractionService" },
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
    }

    private void ConfigureRedis(ServiceConfigurationContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
        context.Services.AddSingleton(redis);
        context.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:Configuration"];
        });
        context.Services.AddDataProtection()
            .SetApplicationName("LCH")
            .PersistKeysToStackExchangeRedis(redis, "LCH-DataProtection-Keys");
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .WithHeaders("*")
                    .WithMethods("*");
            });
        });
    }

    private void ConfigureCAP(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.AddCapEventBus(options =>
        {
            options.UsePostgreSql(configuration["ConnectionStrings:Default"]);
            options.UseRabbitMQ(configuration["RabbitMQ:HostName"], configuration["RabbitMQ:Port"] ?? "5672");
            options.UseDashboard();
        });
    }

    private void ConfigureDistributedLock(ServiceConfigurationContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
        context.Services.AddSingleton<IDistributedLockProvider>(_ => new RedisDistributedSynchronizationProvider(redis.GetDatabase()));
    }

    private void ConfigureExceptionHandling(ServiceConfigurationContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        context.Services.AddExceptionHandling();
    }

    private void ConfigureLocalization(ServiceConfigurationContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        PreConfigure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("zh-CN", "zh-CN", "简体中文"));
        });
    }
}