using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace LCH.Abp.Dapr
{
    [DependsOn(
        typeof(AbpDddApplicationContractsModule))]
    public class AbpDaprTestModule : AbpModule
    {

    }
}
