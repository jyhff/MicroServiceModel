using LCH.Abp.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace LCH.Abp.Gdpr;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpGdprDomainModule),
    typeof(AbpExporterCoreModule),
    typeof(AbpGdprApplicationContractsModule)
    )]
public class AbpGdprApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<AbpGdprApplicationModule>();
    }
}
