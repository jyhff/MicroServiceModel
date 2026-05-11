using LCH.Abp.BlobStoring.Fakes;
using LCH.Abp.BlobStoring.TestObjects;
using LCH.Abp.Tests;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Volo.Abp.Autofac;
using Volo.Abp.BlobStoring;
using Volo.Abp.Modularity;

namespace LCH.Abp.BlobStoring;

[DependsOn(
    typeof(AbpBlobStoringModule),
    typeof(AbpTestsBaseModule),
    typeof(AbpAutofacModule)
    )]
public class AbpBlobStoringTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IBlobProvider>(Substitute.For<FakeBlobProvider1>());
        context.Services.AddSingleton<IBlobProvider>(Substitute.For<FakeBlobProvider2>());

        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers
                .ConfigureDefault(container =>
                {
                    container.SetConfiguration("TestConfigDefault", "TestValueDefault");
                    container.ProviderType = typeof(FakeBlobProvider1);
                })
                .Configure<TestContainer1>(container =>
                {
                    container.SetConfiguration("TestConfig1", "TestValue1");
                    container.ProviderType = typeof(FakeBlobProvider1);
                })
                .Configure<TestContainer2>(container =>
                {
                    container.SetConfiguration("TestConfig2", "TestValue2");
                    container.ProviderType = typeof(FakeBlobProvider2);
                });
        });
    }
}
