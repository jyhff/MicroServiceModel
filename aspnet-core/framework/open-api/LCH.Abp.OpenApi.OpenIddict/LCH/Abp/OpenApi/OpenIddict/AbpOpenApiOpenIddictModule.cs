using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;

namespace LCH.Abp.OpenApi.OpenIddict;

[DependsOn(
    typeof(AbpOpenApiModule),
    typeof(AbpOpenIddictDomainModule))]
public class AbpOpenApiOpenIddictModule : AbpModule
{
}
