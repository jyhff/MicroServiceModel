using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace LCH.Abp.Notifications;

[DependsOn(
    typeof(AbpHttpClientModule),
    typeof(AbpNotificationsApplicationContractsModule))]
public class AbpNotificationsHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddStaticHttpClientProxies(
            typeof(AbpNotificationsApplicationContractsModule).Assembly,
            AbpNotificationsRemoteServiceConsts.RemoteServiceName
        );
    }
}
