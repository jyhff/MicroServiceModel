using LCH.Abp.AspNetCore.Wrapper;
using Volo.Abp.Modularity;

namespace LCH.Abp.AspNetCore.Mvc.Idempotent.Wrapper;

[DependsOn(
    typeof(AbpAspNetCoreWrapperModule),
    typeof(AbpAspNetCoreMvcIdempotentModule))]
public class AbpAspNetCoreMvcIdempotentWrapperModule : AbpModule
{

}
