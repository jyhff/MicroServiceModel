using LCH.Abp.Wrapper;
using Volo.Abp.AspNetCore;
using Volo.Abp.Modularity;

namespace LCH.Abp.AspNetCore.Wrapper;

[DependsOn(
    typeof(AbpWrapperModule),
    typeof(AbpAspNetCoreModule))]
public class AbpAspNetCoreWrapperModule : AbpModule
{

}
