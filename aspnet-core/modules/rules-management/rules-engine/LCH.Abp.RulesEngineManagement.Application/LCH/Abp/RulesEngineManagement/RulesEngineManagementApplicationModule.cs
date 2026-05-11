using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace LCH.Abp.RulesEngineManagement;

[DependsOn(
    typeof(RulesEngineManagementApplicationContractsModule),
    typeof(RulesEngineManagementDomainModule),
    typeof(AbpDddApplicationModule))]
public class RulesEngineManagementApplicationModule : AbpModule
{

}
