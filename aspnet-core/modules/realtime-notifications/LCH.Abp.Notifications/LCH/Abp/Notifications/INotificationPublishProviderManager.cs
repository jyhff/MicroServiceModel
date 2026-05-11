using System.Collections.Generic;

namespace LCH.Abp.Notifications;

public interface INotificationPublishProviderManager
{
    List<INotificationPublishProvider> Providers { get; }
}
