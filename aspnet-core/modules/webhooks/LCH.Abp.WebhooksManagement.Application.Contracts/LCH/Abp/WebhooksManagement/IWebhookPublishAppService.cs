using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace LCH.Abp.WebhooksManagement;

public interface IWebhookPublishAppService : IApplicationService
{
    Task PublishAsync(WebhookPublishInput input);
}
