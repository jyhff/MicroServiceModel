using Volo.Abp.Json;
using Volo.Abp.Modularity;

namespace LCH.Abp.Dapr;

[DependsOn(typeof(AbpJsonModule))]
public class AbpDaprModule : AbpModule
{
}
