using System.Threading.Tasks;

namespace LCH.Abp.Notifications.Templating;
public interface INotificationTemplateResolveContributor
{
    string Name { get; }

    Task ResolveAsync(INotificationTemplateResolveContext context);
}
