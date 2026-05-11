using LCH.MicroService.WebhooksManagement.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.MicroService.WebhooksManagement.DbMigrator;

[DependsOn(
    typeof(WebhooksManagementMigrationsEntityFrameworkCoreModule),
    typeof(AbpAutofacModule)
    )]
public partial class WebhooksManagementDbMigratorModule : AbpModule
{
}
