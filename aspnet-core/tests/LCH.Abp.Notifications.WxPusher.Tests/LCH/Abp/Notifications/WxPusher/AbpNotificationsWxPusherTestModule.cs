using LCH.Abp.WxPusher;
using Volo.Abp.Modularity;

namespace LCH.Abp.Notifications.WxPusher;

[DependsOn(
    typeof(AbpNotificationsWxPusherModule),
    typeof(AbpWxPusherTestModule),
    typeof(AbpNotificationsTestsModule))]
public class AbpNotificationsWxPusherTestModule : AbpModule
{
}
