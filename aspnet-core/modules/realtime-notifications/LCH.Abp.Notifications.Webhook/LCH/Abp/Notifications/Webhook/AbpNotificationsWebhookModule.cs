using LCH.Abp.Webhooks;
using Volo.Abp.Modularity;

namespace LCH.Abp.Notifications.Webhook;

[DependsOn(
    typeof(AbpNotificationsCoreModule),
    typeof(AbpWebhooksModule))]
public class AbpNotificationsWebhookModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpNotificationsPublishOptions>(options =>
        {
            options.PublishProviders.Add<WebhookNotificationPublishProvider>();
        });
    }
}
