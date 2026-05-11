using LCH.Abp.DataProtection.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace LCH.Abp.DataProtectionManagement.EntityFrameworkCore;

[DependsOn(
    typeof(AbpDataProtectionManagementDomainModule),
    typeof(AbpDataProtectionEntityFrameworkCoreModule)
)]
public class AbpDataProtectionManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AbpDataProtectionManagementDbContext>(options =>
        {
            options.AddRepository<EntityTypeInfo, EfCoreEntityTypeInfoRepository>();

            options.AddRepository<RoleEntityRule, EfCoreRoleEntityRuleRepository>();
            options.AddRepository<OrganizationUnitEntityRule, EfCoreOrganizationUnitEntityRuleRepository>();

            options.AddRepository<SubjectStrategy, EfCoreSubjectStrategyRepository>();

            options.AddDefaultRepositories();
        });
    }
}
