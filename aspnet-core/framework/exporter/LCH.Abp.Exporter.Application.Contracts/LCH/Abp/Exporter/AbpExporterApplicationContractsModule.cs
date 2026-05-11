using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace LCH.Abp.Exporter;

[DependsOn(typeof(AbpDddApplicationContractsModule))]
public class AbpExporterApplicationContractsModule : AbpModule
{
}
