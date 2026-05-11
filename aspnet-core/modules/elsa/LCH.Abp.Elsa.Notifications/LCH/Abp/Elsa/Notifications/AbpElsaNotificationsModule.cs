using Elsa;
using LCH.Abp.Notifications;
using Volo.Abp.Modularity;

namespace LCH.Abp.Elsa.Notifications;

[DependsOn(
    typeof(AbpElsaModule),
    typeof(AbpNotificationsModule))]
public class AbpElsaNotificationsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services
            .AddNotificationHandlersFrom<AbpElsaWorkflowNotificationHandler>();
    }
}
