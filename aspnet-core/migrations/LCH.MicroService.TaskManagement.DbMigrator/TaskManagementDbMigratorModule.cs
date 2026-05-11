using LCH.MicroService.TaskManagement.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.MicroService.TaskManagement.DbMigrator;

[DependsOn(
    typeof(TaskManagementMigrationsEntityFrameworkCoreModule),
    typeof(AbpAutofacModule)
    )]
public partial class TaskManagementDbMigratorModule : AbpModule
{
}
