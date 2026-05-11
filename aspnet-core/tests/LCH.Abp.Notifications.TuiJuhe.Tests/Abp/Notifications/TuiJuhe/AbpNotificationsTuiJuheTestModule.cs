using Volo.Abp.Modularity;

namespace LCH.Abp.Notifications.TuiJuhe;

[DependsOn(
    typeof(AbpNotificationsTuiJuheModule),
    typeof(AbpTuiJuheTestModule),
    typeof(AbpNotificationsTestsModule))]
public class AbpNotificationsTuiJuheTestModule : AbpModule
{
}
