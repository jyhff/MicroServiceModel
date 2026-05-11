using System.Threading.Tasks;

namespace LCH.Abp.Notifications;
public interface IStaticNotificationSaver
{
    Task SaveAsync();
}
