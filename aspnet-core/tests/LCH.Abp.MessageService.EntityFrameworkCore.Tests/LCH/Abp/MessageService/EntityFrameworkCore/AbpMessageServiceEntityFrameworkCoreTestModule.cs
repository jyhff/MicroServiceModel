using LCH.Abp.EntityFrameworkCore.Tests;
using Volo.Abp.Modularity;

namespace LCH.Abp.MessageService.EntityFrameworkCore
{
    [DependsOn(
        typeof(AbpMessageServiceEntityFrameworkCoreModule),
        typeof(AbpMessageServiceDomainTestModule),
        typeof(AbpEntityFrameworkCoreTestModule)
        )]
    public class AbpMessageServiceEntityFrameworkCoreTestModule : AbpModule
    {

    }
}
