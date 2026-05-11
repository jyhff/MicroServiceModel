using LCH.Abp.DataProtectionManagement;
using LCH.Abp.DataProtectionManagement.EntityFrameworkCore;
using LCH.Abp.Saas.Editions;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.Saas.Tenants;
using LCH.Abp.TextTemplating;
using LCH.Abp.TextTemplating.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace LCH.Abp.MicroService.AdminService;

[ConnectionStringName("Default")]
public class AdminServiceMigrationsDbContext : 
    AbpDbContext<AdminServiceMigrationsDbContext>,
    ISaasDbContext,
    ITextTemplatingDbContext,
    IFeatureManagementDbContext,
    ISettingManagementDbContext,
    IPermissionManagementDbContext,
    IAbpDataProtectionManagementDbContext
{
    #region Entities from the modules

    public DbSet<EntityTypeInfo> EntityTypeInfos { get; set; }

    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; set; }

    public DbSet<PermissionDefinitionRecord> Permissions { get; set; }

    public DbSet<PermissionGrant> PermissionGrants { get; set; }

    public DbSet<Setting> Settings { get; set; }

    public DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; set; }

    public DbSet<FeatureGroupDefinitionRecord> FeatureGroups { get; set; }

    public DbSet<FeatureDefinitionRecord> Features { get; set; }

    public DbSet<FeatureValue> FeatureValues { get; set; }

    public DbSet<TextTemplate> TextTemplates { get; set; }

    public DbSet<TextTemplateDefinition> TextTemplateDefinitions { get; set; }

    public DbSet<Edition> Editions { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public AdminServiceMigrationsDbContext(DbContextOptions<AdminServiceMigrationsDbContext> options)
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
