using LCH.MicroService.LocalizationManagement.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.MicroService.LocalizationManagement.DbMigrator;

[DependsOn(
    typeof(LocalizationManagementMigrationsEntityFrameworkCoreModule),
    typeof(AbpAutofacModule)
    )]
public partial class LocalizationManagementDbMigratorModule : AbpModule
{
}
