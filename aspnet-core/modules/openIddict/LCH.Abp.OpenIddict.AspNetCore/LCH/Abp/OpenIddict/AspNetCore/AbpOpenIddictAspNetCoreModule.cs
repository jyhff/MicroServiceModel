using LCH.Abp.Identity;
using Volo.Abp.Modularity;
using VoloAbpOpenIddictAspNetCoreModule = Volo.Abp.OpenIddict.AbpOpenIddictAspNetCoreModule;

namespace LCH.Abp.OpenIddict.AspNetCore;

[DependsOn(
    typeof(AbpIdentityDomainSharedModule),
    typeof(VoloAbpOpenIddictAspNetCoreModule))]
[System.Obsolete("The module has been deprecated and will be removed in future versions")]
public class AbpOpenIddictAspNetCoreModule : AbpModule
{
}
