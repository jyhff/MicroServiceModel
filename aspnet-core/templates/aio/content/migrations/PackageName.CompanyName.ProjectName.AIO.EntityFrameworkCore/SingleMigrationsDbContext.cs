using LCH.Abp.DataProtectionManagement.EntityFrameworkCore;
using LCH.Abp.LocalizationManagement.EntityFrameworkCore;
using LCH.Abp.MessageService.EntityFrameworkCore;
using LCH.Abp.Notifications.EntityFrameworkCore;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.TaskManagement.EntityFrameworkCore;
using LCH.Abp.TextTemplating.EntityFrameworkCore;
using LCH.Abp.WebhooksManagement.EntityFrameworkCore;
using LCH.Platform.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PackageName.CompanyName.ProjectName.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.IdentityServer.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace PackageName.CompanyName.ProjectName.AIO.EntityFrameworkCore;

[ConnectionStringName("SingleDbMigrator")]
public class SingleMigrationsDbContext : AbpDbContext<SingleMigrationsDbContext>
{
    public SingleMigrationsDbContext(DbContextOptions<SingleMigrationsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureAuditLogging();
        modelBuilder.ConfigureIdentity();
        modelBuilder.ConfigureIdentityServer();
        modelBuilder.ConfigureOpenIddict();
        modelBuilder.ConfigureSaas();
        modelBuilder.ConfigureFeatureManagement();
        modelBuilder.ConfigureSettingManagement();
        modelBuilder.ConfigurePermissionManagement();
        modelBuilder.ConfigureTextTemplating();
        modelBuilder.ConfigureTaskManagement();
        modelBuilder.ConfigureWebhooksManagement();
        modelBuilder.ConfigurePlatform();
        modelBuilder.ConfigureLocalization();
        modelBuilder.ConfigureNotifications();
        modelBuilder.ConfigureNotificationsDefinition();
        modelBuilder.ConfigureMessageService();
        modelBuilder.ConfigureDataProtectionManagement();
        modelBuilder.ConfigureWebhooksManagement();

        modelBuilder.ConfigureProjectName();
    }
}
