using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace LCH.Abp.LocalizationManagement;

[DependsOn(
    typeof(AbpAuthorizationModule),
    typeof(AbpLocalizationManagementDomainSharedModule))]
public class AbpLocalizationManagementApplicationContractsModule : AbpModule
{

}
