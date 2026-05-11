using Microsoft.Extensions.DependencyInjection;
using LCH.MicroService.WebhooksManagement.DataSeeder;

namespace LCH.MicroService.WebhooksManagement;

public partial class WebhooksManagementHttpApiHostModule
{
    private static void ConfigureSeedWorker(IServiceCollection services, bool isDevelopment = false)
    {
        if (isDevelopment)
        {
            services.AddHostedService<WebhooksManagementDataSeederWorker>();
        }
    }
}
