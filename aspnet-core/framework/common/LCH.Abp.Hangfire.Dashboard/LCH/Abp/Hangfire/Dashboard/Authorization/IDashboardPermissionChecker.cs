using Hangfire.Dashboard;
using System.Threading.Tasks;

namespace LCH.Abp.Hangfire.Dashboard.Authorization;

public interface IDashboardPermissionChecker
{
    Task<bool> IsGrantedAsync(DashboardContext context, string[] requiredPermissionNames);
}
