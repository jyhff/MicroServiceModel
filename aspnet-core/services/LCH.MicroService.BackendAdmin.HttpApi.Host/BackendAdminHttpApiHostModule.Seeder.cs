using LCH.MicroService.BackendAdmin.DataSeeder;
using Microsoft.Extensions.DependencyInjection;

namespace LCH.MicroService.BackendAdmin;

public partial class BackendAdminHttpApiHostModule
{
    private static void ConfigureSeedWorker(IServiceCollection services, bool isDevelopment = false)
    {
        if (isDevelopment)
        {
            services.AddHostedService<BackendAdminDataSeederWorker>();
        }
    }
}
