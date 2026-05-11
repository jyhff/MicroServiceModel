using LCH.Abp.IM;
using Volo.Abp.Modularity;

namespace LCH.Abp.Elsa.Activities.IM;

[DependsOn(
    typeof(AbpElsaModule),
    typeof(AbpIMModule))]
public class AbpElsaActivitiesIMModule : AbpModule
{
}
