using LCH.Abp.DataProtectionManagement.EntityFrameworkCore;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.TextTemplating.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace LCH.MicroService.BackendAdmin.EntityFrameworkCore;

[ConnectionStringName("BackendAdminDbMigrator")]
public class BackendAdminMigrationsDbContext : AbpDbContext<BackendAdminMigrationsDbContext>
{
    public BackendAdminMigrationsDbContext(DbContextOptions<BackendAdminMigrationsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureSaas();
        modelBuilder.ConfigureTextTemplating();
        modelBuilder.ConfigureFeatureManagement();
        modelBuilder.ConfigureSettingManagement();
        modelBuilder.ConfigurePermissionManagement();
        modelBuilder.ConfigureDataProtectionManagement();
    }
}
