using LCH.Abp.Tests;
using LCH.Platform.HttpApi.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace LCH.Abp.Sms.Platform;

[DependsOn(
    typeof(PlatformHttpApiClientModule),
    typeof(AbpSmsPlatformModule),
    typeof(AbpTestsBaseModule))]
public class AbpSmsPlatformTestsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configurationOptions = new AbpConfigurationBuilderOptions
        {
            BasePath = @"D:\Projects\Development\Abp\Sms\Platform",
            EnvironmentName = "Test"
        };

        var configuration = ConfigurationHelper.BuildConfiguration(configurationOptions);

        context.Services.ReplaceConfiguration(configuration);
    }
}
