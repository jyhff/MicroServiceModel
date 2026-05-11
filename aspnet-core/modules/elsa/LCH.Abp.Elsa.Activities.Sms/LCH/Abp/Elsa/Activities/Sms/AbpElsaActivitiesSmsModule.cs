using Volo.Abp.Modularity;
using Volo.Abp.Sms;

namespace LCH.Abp.Elsa.Activities.Sms;

[DependsOn(
    typeof(AbpElsaModule),
    typeof(AbpSmsModule))]
public class AbpElsaActivitiesSmsModule : AbpModule
{
}
