using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCH.Abp.Notifications;

public interface IStaticNotificationDefinitionStore
{
    Task<NotificationDefinition> GetOrNullAsync(string name);

    Task<IReadOnlyList<NotificationDefinition>> GetNotificationsAsync();

    Task<NotificationGroupDefinition> GetGroupOrNullAsync(string name);

    Task<IReadOnlyList<NotificationGroupDefinition>> GetGroupsAsync();
}
