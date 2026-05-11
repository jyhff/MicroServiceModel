using System.Threading.Tasks;

namespace LCH.Abp.Notifications.Webhook;
public interface IWebhookNotificationContributor
{
    string Name { get; }
    Task ContributeAsync(IWebhookNotificationContext context);
}
