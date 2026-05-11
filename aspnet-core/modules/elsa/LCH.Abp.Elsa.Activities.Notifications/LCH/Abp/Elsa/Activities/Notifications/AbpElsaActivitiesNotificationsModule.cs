using LCH.Abp.Notifications;
using Volo.Abp.Modularity;

namespace LCH.Abp.Elsa.Activities.Notifications;

[DependsOn(
    typeof(AbpElsaModule),
    typeof(AbpNotificationsModule))]
public class AbpElsaActivitiesNotificationsModule : AbpModule
{
}
