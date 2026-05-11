using LCH.Abp.Saas;
using Volo.Abp.Caching;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace LCH.Abp.MultiTenancy.Saas;

[DependsOn(typeof(AbpCachingModule))]
[DependsOn(typeof(AbpEventBusModule))]
[DependsOn(typeof(AbpSaasHttpApiClientModule))]
public class AbpMultiTenancySaasModule : AbpModule
{
}
