using System;
using Elastic.Clients.Elasticsearch;
using LCH.Bilibili.Search;
using LCH.Bilibili.Search.Elasticsearch.Indexing;
using LCH.Bilibili.Search.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.BasicTheme;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace LCH.MicroService.Search;

[DependsOn(
    typeof(SearchHttpApiModule),
    typeof(SearchApplicationModule),
    typeof(SearchEntityFrameworkCoreModule),
    typeof(SearchHttpApiClientModule),
    typeof(AbpAspNetCoreMvcUiBasicThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpSwashbuckleModule))]
public class SearchServiceHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        ConfigureSwagger(context, configuration);
        ConfigureElasticsearch(context, configuration);
    }

    private void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Search Service API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.CustomSchemaIds(type => type.FullName);
        });
    }

    private void ConfigureElasticsearch(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var elasticsearchUrl = configuration["Elasticsearch:Url"] ?? "http://localhost:9200";

        context.Services.AddSingleton<ElasticsearchClient>(sp =>
        {
            var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl))
                .DefaultIndex(VideoSearchIndexConfiguration.IndexName);

            return new ElasticsearchClient(settings);
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();

        app.UseAbpRequestLocalization();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Search Service API");
        });
        app.UseAuthorization();
        app.UseConfiguredEndpoints();

        if (context.ServiceProvider.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            CreateElasticsearchIndexAsync(context.ServiceProvider).GetAwaiter().GetResult();
        }
    }

    private async Task CreateElasticsearchIndexAsync(IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<ElasticsearchClient>();
        await VideoSearchIndexConfiguration.CreateIndexAsync(client);
    }
}