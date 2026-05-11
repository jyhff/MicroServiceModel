using LCH.Abp.DataProtection;
using LCH.Abp.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace LCH.Abp.Demo;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpDataProtectionModule),
    typeof(AbpDemoDomainModule),
    typeof(AbpExporterApplicationModule),
    typeof(AbpDemoApplicationContractsModule))]
public class AbpDemoApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<AbpDemoApplicationModule>();
    }
}
