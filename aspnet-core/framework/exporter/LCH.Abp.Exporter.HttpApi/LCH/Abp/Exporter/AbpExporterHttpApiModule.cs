using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace LCH.Abp.Exporter;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpExporterApplicationContractsModule))]
public class AbpExporterHttpApiModule : AbpModule
{

}
