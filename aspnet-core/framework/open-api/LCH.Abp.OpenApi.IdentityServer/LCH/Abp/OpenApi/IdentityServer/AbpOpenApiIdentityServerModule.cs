using Volo.Abp.IdentityServer;
using Volo.Abp.Modularity;

namespace LCH.Abp.OpenApi.IdentityServer;

[DependsOn(
    typeof(AbpOpenApiModule),
    typeof(AbpIdentityServerDomainModule))]
public class AbpOpenApiIdentityServerModule : AbpModule
{
}
