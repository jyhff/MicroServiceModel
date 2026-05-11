using Hangfire;
using Volo.Abp.DependencyInjection;

namespace LCH.Abp.Hangfire.Dashboard;

public class AbpHangfireDashboardOptionsProvider : ITransientDependency
{
    public virtual DashboardOptions Get()
    {
        return new DashboardOptions();
    }
}
