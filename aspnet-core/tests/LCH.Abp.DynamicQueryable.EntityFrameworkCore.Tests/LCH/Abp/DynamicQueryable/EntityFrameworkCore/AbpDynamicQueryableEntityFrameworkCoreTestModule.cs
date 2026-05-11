using Volo.Abp.Modularity;

namespace LCH.Abp.DynamicQueryable.EntityFrameworkCore;

[DependsOn(typeof(AbpEntityFrameworkCoreTestModule))]
public class AbpDynamicQueryableEntityFrameworkCoreTestModule : AbpModule
{

}