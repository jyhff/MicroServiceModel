using LCH.Abp.Wrapper;
using Volo.Abp.AspNetCore;
using Volo.Abp.Modularity;
using Volo.Abp.Timing;

namespace LCH.Abp.OpenApi.Authorization
{
    [DependsOn(
        typeof(AbpWrapperModule),
        typeof(AbpTimingModule),
        typeof(AbpOpenApiModule),
        typeof(AbpAspNetCoreModule))]
    public class AbpOpenApiAuthorizationModule : AbpModule
    {
    }
}
