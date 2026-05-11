using Volo.Abp.Modularity;

namespace LCH.Abp.IdentityServer.EntityFrameworkCore;

[DependsOn(typeof(LCH.Abp.IdentityServer.AbpIdentityServerDomainModule))]
[DependsOn(typeof(Volo.Abp.IdentityServer.EntityFrameworkCore.AbpIdentityServerEntityFrameworkCoreModule))]
public class AbpIdentityServerEntityFrameworkCoreModule : AbpModule
{
}
