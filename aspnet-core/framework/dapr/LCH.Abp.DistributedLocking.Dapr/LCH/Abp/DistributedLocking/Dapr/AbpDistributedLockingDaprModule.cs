using LCH.Abp.Dapr;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace LCH.Abp.DistributedLocking.Dapr;

[DependsOn(
    typeof(AbpDaprModule),
    typeof(AbpThreadingModule),
    typeof(AbpDistributedLockingAbstractionsModule))]
public class AbpDistributedLockingDaprModule : AbpModule
{

}
