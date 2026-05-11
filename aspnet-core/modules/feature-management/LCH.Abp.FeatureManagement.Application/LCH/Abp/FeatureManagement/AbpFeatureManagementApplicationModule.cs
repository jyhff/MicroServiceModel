using Volo.Abp.Modularity;
using VoloAbpFeatureManagementApplicationModule = Volo.Abp.FeatureManagement.AbpFeatureManagementApplicationModule;

namespace LCH.Abp.FeatureManagement;

[DependsOn(
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(VoloAbpFeatureManagementApplicationModule))]
public class AbpFeatureManagementApplicationModule : AbpModule
{
}
