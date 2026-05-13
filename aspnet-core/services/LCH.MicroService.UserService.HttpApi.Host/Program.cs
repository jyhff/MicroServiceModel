using LCH.MicroService.UserService;
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

namespace LY.MicroService.UserService;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        try
        {
            Console.Title = "UserService.HttpApi.Host";
            Log.Information("Starting UserService.HttpApi.Host.");

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // AgileConfig is optional, can be added later if needed
                })
                .UseSerilog((context, provider, config) =>
                {
                    config.ReadFrom.Configuration(context.Configuration);
                });
            await builder.AddApplicationAsync<UserServiceHttpApiHostModule>(options =>
            {
                UserServiceHttpApiHostModule.ApplicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME")
                    ?? UserServiceHttpApiHostModule.ApplicationName;
                options.ApplicationName = UserServiceHttpApiHostModule.ApplicationName;
                // 从环境变量获取用户机密标识, 如果未指定用户机密, 从项目获取
                options.Configuration.UserSecretsId = Environment.GetEnvironmentVariable("APPLICATION_USER_SECRETS_ID");
                // 如果用户未指定用户机密, 从项目获取
                options.Configuration.UserSecretsAssembly = typeof(UserServiceHttpApiHostModule).Assembly;
                // 加载 Modules 目录下的程序集文件作为插件
                // 取消表示仅加载当前项目模块，作为通过程序集引用方式加载
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
            Log.Fatal(ex, "UserService.HttpApi.Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}