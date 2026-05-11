using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace LCH.Abp.Gdpr;

[DependsOn(
    typeof(AbpAuthorizationAbstractionsModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpGdprDomainSharedModule)
    )]
public class AbpGdprApplicationContractsModule : AbpModule
{
}
