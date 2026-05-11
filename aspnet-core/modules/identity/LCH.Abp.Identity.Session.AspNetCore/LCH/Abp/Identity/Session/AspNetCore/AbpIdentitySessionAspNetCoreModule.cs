using LCH.Abp.IP.Location;
using Volo.Abp.AspNetCore;
using Volo.Abp.Modularity;

namespace LCH.Abp.Identity.Session.AspNetCore;

[DependsOn(typeof(AbpAspNetCoreModule))]
[DependsOn(typeof(AbpIPLocationModule))]
[DependsOn(typeof(AbpIdentitySessionModule))]
public class AbpIdentitySessionAspNetCoreModule : AbpModule
{
}
