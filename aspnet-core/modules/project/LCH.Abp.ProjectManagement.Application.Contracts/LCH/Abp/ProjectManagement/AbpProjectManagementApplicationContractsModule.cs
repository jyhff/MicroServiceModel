using LCH.Abp.ProjectManagement.Localization;
using Volo.Abp.Application;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.ProjectManagement
{
    [DependsOn(
        typeof(AbpDddApplicationContractsModule),
        typeof(AbpProjectManagementDomainSharedModule))]
    public class AbpProjectManagementApplicationContractsModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<AbpProjectManagementApplicationContractsModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Get<AbpProjectManagementResource>()
                    .AddVirtualJson("/LCH/Abp/ProjectManagement/Localization/ApplicationContracts");
            });
        }
    }
}
