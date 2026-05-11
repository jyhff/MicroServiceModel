using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.Abp.MicroService.AdminService;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AdminServiceMigrationsEntityFrameworkCoreModule)
    )]
public class AdminServiceDbMigratorModule : AbpModule
{

}
