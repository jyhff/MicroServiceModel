using LCH.MicroService.IdentityServer.DataSeeder;
using Microsoft.Extensions.DependencyInjection;

namespace LCH.MicroService.IdentityServer;

public partial class IdentityServerModule
{
    private static void ConfigureSeedWorker(IServiceCollection services, bool isDevelopment = false)
    {
        if (isDevelopment)
        {
            services.AddHostedService<IdentityServerDataSeederWorker>();
        }
    }
}
