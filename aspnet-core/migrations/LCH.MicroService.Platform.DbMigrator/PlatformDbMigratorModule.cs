using LCH.MicroService.Platform.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.MicroService.Platform.DbMigrator;

[DependsOn(
    typeof(PlatformMigrationsEntityFrameworkCoreModule),
    typeof(AbpAutofacModule)
    )]
public partial class PlatformDbMigratorModule : AbpModule
{
}
