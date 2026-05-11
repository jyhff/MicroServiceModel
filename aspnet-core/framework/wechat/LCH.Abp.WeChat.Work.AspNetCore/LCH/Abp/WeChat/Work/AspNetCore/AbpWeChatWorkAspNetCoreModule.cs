using Volo.Abp.AspNetCore;
using Volo.Abp.Modularity;

namespace LCH.Abp.WeChat.Work.AspNetCore;

[DependsOn(
    typeof(AbpWeChatWorkModule),
    typeof(AbpAspNetCoreModule))]
public class AbpWeChatWorkAspNetCoreModule : AbpModule
{
}
