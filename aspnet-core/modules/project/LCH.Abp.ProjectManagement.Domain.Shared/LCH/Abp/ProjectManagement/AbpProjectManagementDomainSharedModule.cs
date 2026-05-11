using LCH.Abp.ProjectManagement.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.ProjectManagement
{
    [DependsOn(
        typeof(AbpValidationModule))]
    public class AbpProjectManagementDomainSharedModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<AbpProjectManagementDomainSharedModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<AbpProjectManagementResource>()
                    .AddBaseTypes(typeof(AbpValidationResource))
                    .AddVirtualJson("/LCH/Abp/ProjectManagement/Localization/Domain");
            });
        }
    }
}
