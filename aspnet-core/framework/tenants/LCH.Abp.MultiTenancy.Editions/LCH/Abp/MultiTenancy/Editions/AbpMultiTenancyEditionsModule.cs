using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace LCH.Abp.MultiTenancy.Editions;

[DependsOn(typeof(AbpMultiTenancyModule))]
public class AbpMultiTenancyEditionsModule : AbpModule
{
}
