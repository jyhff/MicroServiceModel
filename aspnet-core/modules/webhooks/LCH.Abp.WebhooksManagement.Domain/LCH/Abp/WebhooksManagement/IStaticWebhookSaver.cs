using System.Threading.Tasks;

namespace LCH.Abp.WebhooksManagement;

public interface IStaticWebhookSaver
{
    Task SaveAsync();
}
