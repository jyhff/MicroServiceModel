using System.Collections.Generic;

namespace LCH.Abp.Notifications.Webhook;
public class AbpNotificationsWebhookOptions
{
    public IList<IWebhookNotificationContributor> Contributors { get; }
    public AbpNotificationsWebhookOptions()
    {
        Contributors = new List<IWebhookNotificationContributor>();
    }
}
