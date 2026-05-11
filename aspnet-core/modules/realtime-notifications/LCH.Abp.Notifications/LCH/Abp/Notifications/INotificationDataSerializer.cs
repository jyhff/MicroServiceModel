using System.Threading.Tasks;

namespace LCH.Abp.Notifications;
public interface INotificationDataSerializer
{
    NotificationData Serialize(NotificationData source);
    Task<NotificationStandardData> ToStandard(NotificationData source);
}
