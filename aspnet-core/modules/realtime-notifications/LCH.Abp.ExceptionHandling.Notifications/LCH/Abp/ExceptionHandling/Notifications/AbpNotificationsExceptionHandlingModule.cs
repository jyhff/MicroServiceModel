using LCH.Abp.Notifications.Common;
using Volo.Abp.Modularity;

namespace LCH.Abp.ExceptionHandling.Notifications;

[DependsOn(
    typeof(AbpExceptionHandlingModule),
    typeof(AbpNotificationsCommonModule))]
public class AbpNotificationsExceptionHandlingModule : AbpModule
{
}
