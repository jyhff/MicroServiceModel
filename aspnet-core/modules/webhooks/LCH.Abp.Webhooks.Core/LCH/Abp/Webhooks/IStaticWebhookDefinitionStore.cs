using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCH.Abp.Webhooks;

public interface IStaticWebhookDefinitionStore
{
    Task<WebhookDefinition> GetOrNullAsync(string name);

    Task<IReadOnlyList<WebhookDefinition>> GetWebhooksAsync();

    Task<WebhookGroupDefinition> GetGroupOrNullAsync(string name);

    Task<IReadOnlyList<WebhookGroupDefinition>> GetGroupsAsync();
}
