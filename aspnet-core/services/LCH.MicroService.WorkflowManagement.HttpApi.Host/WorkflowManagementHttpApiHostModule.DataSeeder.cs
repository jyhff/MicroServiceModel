using LCH.MicroService.WorkflowManagement.DataSeeder;
using Microsoft.Extensions.DependencyInjection;

namespace LCH.MicroService.WorkflowManagement;

public partial class WorkflowManagementHttpApiHostModule
{
    private static void ConfigureSeedWorker(IServiceCollection services, bool isDevelopment = false)
    {
        if (isDevelopment)
        {
            services.AddHostedService<WorkflowManagementDataSeederWorker>();
        }
    }
}
