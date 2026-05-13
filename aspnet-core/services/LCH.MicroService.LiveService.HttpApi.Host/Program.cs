using LCH.MicroService.LiveService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.IO;
using Volo.Abp.Modularity.PlugIns;

namespace LY.MicroService.LiveService;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        try
        {
            Console.Title = "LiveService.HttpApi.Host";
            Log.Information("Starting LiveService.HttpApi.Host.");

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .ConfigureAppConfiguration((context, config) =>
                {
                })
                .UseSerilog((context, provider, config) =>
                {
                    config.ReadFrom.Configuration(context.Configuration);
                });
            await builder.AddApplicationAsync<LiveServiceHttpApiHostModule>(options =>
            {
                LiveServiceHttpApiHostModule.ApplicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME")
                    ?? LiveServiceHttpApiHostModule.ApplicationName;
                options.ApplicationName = LiveServiceHttpApiHostModule.ApplicationName;
                options.Configuration.UserSecretsId = Environment.GetEnvironmentVariable("APPLICATION_USER_SECRETS_ID");
                options.Configuration.UserSecretsAssembly = typeof(LiveServiceHttpApiHostModule).Assembly;
                var pluginFolder = Path.Combine(
                        Directory.GetCurrentDirectory(), "Modules");
                DirectoryHelper.CreateIfNotExists(pluginFolder);
                options.PlugInSources.AddFolder(
                    pluginFolder,
                    SearchOption.AllDirectories);
            });
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "LiveService.HttpApi.Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}