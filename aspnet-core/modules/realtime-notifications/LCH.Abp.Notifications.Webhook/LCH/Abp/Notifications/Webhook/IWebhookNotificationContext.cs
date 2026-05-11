using Volo.Abp.DependencyInjection;

namespace LCH.Abp.Notifications.Webhook;
public interface IWebhookNotificationContext : IServiceProviderAccessor
{
    WebhookNotificationData Webhook { get; set; }
    NotificationInfo Notification { get; }
    bool Handled { get; set; }
}
