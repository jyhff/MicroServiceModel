using LCH.Abp.Webhooks;
using Volo.Abp.Modularity;

namespace LCH.Abp.Elsa.Activities.Webhooks;

[DependsOn(
    typeof(AbpElsaModule),
    typeof(AbpWebhooksModule))]
public class AbpElsaActivitiesWebhooksModule : AbpModule
{
}
