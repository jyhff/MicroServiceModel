using LCH.Abp.DataProtection;
using LCH.Abp.Exporter;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace LCH.Abp.Demo;

[DependsOn(
    typeof(AbpDataProtectionAbstractionsModule),
    typeof(AbpExporterApplicationContractsModule),
    typeof(AbpAuthorizationAbstractionsModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpDemoDomainSharedModule))]
public class AbpDemoApplicationContractsModule : AbpModule
{
}
