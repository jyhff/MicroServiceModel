using LCH.Abp.WebhooksManagement;
using Volo.Abp.Modularity;

namespace LCH.Abp.Webhooks.ClientProxies;

[DependsOn(typeof(AbpWebhooksModule))]
[DependsOn(typeof(WebhooksManagementHttpApiClientModule))]
public class AbpWebHooksClientProxiesModule : AbpModule
{
}
