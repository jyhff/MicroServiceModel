using LCH.Abp.DataProtection.EntityFrameworkCore;
using LCH.Abp.Demo.Authors;
using LCH.Abp.Demo.Books;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace LCH.Abp.Demo.EntityFrameworkCore;

[DependsOn(
    typeof(AbpDemoDomainModule),
    typeof(AbpDataProtectionEntityFrameworkCoreModule))]
public class AbpDemoEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<DemoDbContext>(options =>
        {
            // 
            options.AddRepository<Author, EfCoreAuthorRepository>();
            options.AddRepository<Book, EfCoreBookRepository>();

            options.AddDefaultRepositories();
        });
    }
}
