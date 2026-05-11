using LCH.Abp.DataProtection.EntityFrameworkCore;
using LCH.Abp.EntityFrameworkCore.Tests;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace LCH.Abp.DataProtection
{
    [DependsOn(
        typeof(AbpDataProtectionEntityFrameworkCoreModule),
        typeof(AbpEntityFrameworkCoreTestModule))]
    public class AbpDataProtectionTestModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<AbpDataProtectionTestDbContext>(options =>
            {
                options.AddRepository<FakeProtectionObject, EfCoreFakeProtectionObjectRepository>();
            });
        }
    }
}
