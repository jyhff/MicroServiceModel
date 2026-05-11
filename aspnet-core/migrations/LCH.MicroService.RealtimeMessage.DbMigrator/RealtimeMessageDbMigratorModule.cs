using LCH.Abp.Data.DbMigrator;
using LCH.MicroService.RealtimeMessage.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.MicroService.RealtimeMessage.DbMigrator;

[DependsOn(
    typeof(RealtimeMessageMigrationsEntityFrameworkCoreModule),
    typeof(AbpDataDbMigratorModule),
    typeof(AbpAutofacModule)
    )]
public partial class RealtimeMessageDbMigratorModule : AbpModule
{
}
