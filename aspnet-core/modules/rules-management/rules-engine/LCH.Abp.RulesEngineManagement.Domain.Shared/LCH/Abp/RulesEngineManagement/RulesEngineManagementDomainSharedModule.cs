using Volo.Abp.Features;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace LCH.Abp.RulesEngineManagement;

[DependsOn(
    typeof(AbpFeaturesModule),
    typeof(AbpValidationModule))]
public class RulesEngineManagementDomainSharedModule : AbpModule
{

}
