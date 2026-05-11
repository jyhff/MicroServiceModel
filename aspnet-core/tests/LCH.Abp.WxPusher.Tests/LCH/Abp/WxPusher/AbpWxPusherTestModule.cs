using LCH.Abp.Tests;
using LCH.Abp.Tests.Features;
using LCH.Abp.WxPusher.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace LCH.Abp.WxPusher;

[DependsOn(
        typeof(AbpWxPusherModule),
        typeof(AbpTestsBaseModule))]
public class AbpWxPusherTestModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configurationOptions = new AbpConfigurationBuilderOptions
        {
            BasePath = @"D:\Projects\Development\Abp\WxPusher",
            EnvironmentName = "Test"
        };
        var configuration = ConfigurationHelper.BuildConfiguration(configurationOptions);

        context.Services.ReplaceConfiguration(configuration);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FakeFeatureOptions>(options =>
        {
            options.Map(WxPusherFeatureNames.Enable, (_) => "true");
            options.Map(WxPusherFeatureNames.Message.Enable, (_) => "true");
            options.Map(WxPusherFeatureNames.Message.SendLimit, (_) => "500");
            options.Map(WxPusherFeatureNames.Message.SendLimitInterval, (_) => "1");
        });
    }
}
