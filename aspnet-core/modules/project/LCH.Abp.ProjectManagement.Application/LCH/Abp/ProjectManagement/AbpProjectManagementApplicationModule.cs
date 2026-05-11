using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace LCH.Abp.ProjectManagement
{
    [DependsOn(
        typeof(AbpDddApplicationModule),
        typeof(AbpProjectManagementApplicationContractsModule))]
    public class AbpProjectManagementApplicationModule : AbpModule
    {
    }
}
