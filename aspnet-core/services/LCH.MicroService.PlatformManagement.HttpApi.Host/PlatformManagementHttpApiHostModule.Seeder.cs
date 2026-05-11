using LCH.MicroService.PlatformManagement.DataSeeder;
using Microsoft.Extensions.DependencyInjection;

namespace LCH.MicroService.PlatformManagement;

public partial class PlatformManagementHttpApiHostModule
{
    private static void ConfigureSeedWorker(IServiceCollection services, bool isDevelopment = false)
    {
        if (isDevelopment)
        {
            services.AddHostedService<PlatformManagementDataSeederWorker>();
        }
    }
}
