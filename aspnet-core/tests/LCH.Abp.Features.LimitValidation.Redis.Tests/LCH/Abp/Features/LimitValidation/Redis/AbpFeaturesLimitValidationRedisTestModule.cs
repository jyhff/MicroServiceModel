using LCH.Abp.Tests;
using Volo.Abp.Modularity;

namespace LCH.Abp.Features.LimitValidation.Redis
{
    [DependsOn(
        typeof(AbpFeaturesLimitValidationTestModule),
        typeof(AbpFeaturesValidationRedisModule),
        typeof(AbpTestsBaseModule))]
    public class AbpFeaturesLimitValidationRedisTestModule : AbpModule
    {
    }
}
