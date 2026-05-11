using LCH.Abp.Rules.RulesEngine;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace LCH.Abp.RulesEngineManagement;

[DependsOn(
    typeof(RulesEngineManagementDomainSharedModule),
    typeof(AbpRulesEngineModule),
    typeof(AbpDddDomainModule))]
public class RulesEngineManagementDomainModule : AbpModule
{

}
