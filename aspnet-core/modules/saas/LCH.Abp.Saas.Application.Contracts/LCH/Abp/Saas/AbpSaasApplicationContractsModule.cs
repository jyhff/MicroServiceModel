using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace LCH.Abp.Saas;

[DependsOn(typeof(AbpSaasDomainSharedModule))]
[DependsOn(typeof(AbpAuthorizationAbstractionsModule))]
[DependsOn(typeof(AbpDddApplicationContractsModule))]
public class AbpSaasApplicationContractsModule : AbpModule
{
}
