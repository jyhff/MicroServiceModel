using LCH.Abp.EntityFrameworkCore.Tests;
using Volo.Abp.Modularity;

namespace LCH.Platform.EntityFrameworkCore
{
    [DependsOn(
        typeof(PlatformDomainTestModule),
        typeof(PlatformEntityFrameworkCoreModule),
        typeof(AbpEntityFrameworkCoreTestModule)
        )]
    public class PlatformEntityFrameworkCoreTestModule : AbpModule
    {
    }
}
