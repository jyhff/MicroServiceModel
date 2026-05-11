using LCH.Abp.Tests;
using Volo.Abp.Modularity;

namespace LCH.Abp.Wrapper
{
    [DependsOn(
        typeof(AbpWrapperModule),
        typeof(AbpTestsBaseModule))]
    public class AbpWrapperTestsModule: AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpWrapperOptions>(options =>
            {
                options.AddHandler<FakeException>(new FakeExceptionWrapHandler());
            });
        }
    }
}
