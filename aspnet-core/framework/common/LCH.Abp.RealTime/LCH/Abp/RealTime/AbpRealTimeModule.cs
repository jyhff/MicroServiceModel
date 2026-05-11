using Volo.Abp.EventBus.Abstractions;
using Volo.Abp.Modularity;

namespace LCH.Abp.RealTime;

[DependsOn(typeof(AbpEventBusAbstractionsModule))]
public class AbpRealTimeModule : AbpModule
{
}
