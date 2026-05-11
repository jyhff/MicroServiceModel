using LCH.MicroService.AuthServer.DataSeeder;
using Microsoft.Extensions.DependencyInjection;

namespace LCH.MicroService.AuthServer;

public partial class AuthServerModule
{
    private static void ConfigureSeedWorker(IServiceCollection services, bool isDevelopment = false)
    {
        if (isDevelopment)
        {
            services.AddHostedService<AuthServerDataSeederWorker>();
        }
    }
}
