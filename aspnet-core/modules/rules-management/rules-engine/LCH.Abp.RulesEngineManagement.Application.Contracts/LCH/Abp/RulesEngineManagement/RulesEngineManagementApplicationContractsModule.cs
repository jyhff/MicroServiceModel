using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace LCH.Abp.RulesEngineManagement;

[DependsOn(
    typeof(RulesEngineManagementDomainSharedModule),
    typeof(AbpDddApplicationContractsModule))]
public class RulesEngineManagementApplicationContractsModule : AbpModule
{

}
