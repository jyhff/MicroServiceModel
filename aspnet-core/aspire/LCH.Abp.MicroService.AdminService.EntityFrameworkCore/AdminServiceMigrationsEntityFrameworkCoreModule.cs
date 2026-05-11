using LCH.Abp.Account;
using LCH.Abp.Auditing;
using LCH.Abp.CachingManagement;
using LCH.Abp.Data.DbMigrator;
using LCH.Abp.DataProtectionManagement;
using LCH.Abp.DataProtectionManagement.EntityFrameworkCore;
using LCH.Abp.FeatureManagement;
using LCH.Abp.Gdpr;
using LCH.Abp.Identity;
using LCH.Abp.Identity.EntityFrameworkCore;
using LCH.Abp.IdentityServer;
using LCH.Abp.LocalizationManagement;
using LCH.Abp.LocalizationManagement.EntityFrameworkCore;
using LCH.Abp.MessageService;
using LCH.Abp.Notifications;
using LCH.Abp.OpenIddict;
using LCH.Abp.OssManagement;
using LCH.Abp.ProjectManagement;
using LCH.Abp.RulesEngineManagement;
using LCH.Abp.Saas;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.SettingManagement;
using LCH.Abp.TaskManagement;
using LCH.Abp.TextTemplating;
using LCH.Abp.TextTemplating.EntityFrameworkCore;
using LCH.Abp.WebhooksManagement;
using LCH.Platform;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace LCH.Abp.MicroService.AdminService;

[DependsOn(
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpAuditingApplicationContractsModule),
    typeof(AbpCachingManagementApplicationContractsModule),
    typeof(AbpDataProtectionManagementApplicationContractsModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpGdprApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpIdentityServerApplicationContractsModule),
    typeof(AbpLocalizationManagementApplicationContractsModule),
    typeof(AbpOpenIddictApplicationContractsModule),
    typeof(AbpOssManagementApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(PlatformApplicationContractModule),
    typeof(AbpProjectManagementApplicationContractsModule),
    typeof(AbpMessageServiceApplicationContractsModule),
    typeof(AbpNotificationsApplicationContractsModule),
    typeof(RulesEngineManagementApplicationContractsModule),
    typeof(AbpSaasApplicationContractsModule),
    typeof(TaskManagementApplicationContractsModule),
    typeof(AbpTextTemplatingApplicationContractsModule),
    typeof(WebhooksManagementApplicationContractsModule))]

[DependsOn(
    typeof(AbpSaasEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpDataProtectionManagementEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),// 用户角色权限需要引用包
    typeof(AbpLocalizationManagementEntityFrameworkCoreModule),
    typeof(AbpTextTemplatingEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpDataDbMigratorModule)
    )]
public class AdminServiceMigrationsEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AdminServiceMigrationsDbContext>();

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });
    }
}
