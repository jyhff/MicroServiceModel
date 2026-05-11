using LCH.Abp.Auditing;
using LCH.Abp.CachingManagement;
using LCH.Abp.Identity;
using LCH.Abp.IdentityServer;
using LCH.Abp.LocalizationManagement;
using LCH.Abp.MessageService;
using LCH.Abp.Notifications;
using LCH.Abp.OpenIddict;
using LCH.Abp.OssManagement;
using LCH.Abp.SettingManagement;
using LCH.Abp.TaskManagement;
using LCH.Abp.TextTemplating;
using LCH.Abp.WebhooksManagement;
using LCH.Platform;
using LCH.MicroService.BackendAdmin.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;

namespace LCH.MicroService.BackendAdmin.DbMigrator;

[DependsOn(
    typeof(BackendAdminMigrationsEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(AbpLocalizationManagementApplicationContractsModule),
    typeof(AbpCachingManagementApplicationContractsModule),
    typeof(AbpAuditingApplicationContractsModule),
    typeof(AbpTextTemplatingApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpIdentityServerApplicationContractsModule),
    typeof(AbpOpenIddictApplicationContractsModule),
    typeof(PlatformApplicationContractModule),
    typeof(AbpOssManagementApplicationContractsModule),
    typeof(AbpNotificationsApplicationContractsModule),
    typeof(AbpMessageServiceApplicationContractsModule),
    typeof(TaskManagementApplicationContractsModule),
    typeof(WebhooksManagementApplicationContractsModule),
    typeof(AbpAutofacModule)
    )]
public partial class BackendAdminDbMigratorModule : AbpModule
{
}
