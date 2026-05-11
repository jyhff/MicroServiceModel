using LCH.Abp.WxPusher;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;

namespace LCH.Abp.Identity.WxPusher;

[DependsOn(
    typeof(AbpWxPusherModule),
    typeof(AbpIdentityDomainModule))]
public class AbpIdentityWxPusherModule : AbpModule
{
}
