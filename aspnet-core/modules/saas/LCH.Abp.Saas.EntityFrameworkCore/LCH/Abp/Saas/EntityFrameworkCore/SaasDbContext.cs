using LCH.Abp.Saas.Editions;
using LCH.Abp.Saas.Tenants;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace LCH.Abp.Saas.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(AbpSaasDbProperties.ConnectionStringName)]
public class SaasDbContext : AbpDbContext<SaasDbContext>, ISaasDbContext
{
    public DbSet<Edition> Editions { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    public SaasDbContext(DbContextOptions<SaasDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.SetMultiTenancySide(MultiTenancySides.Host);

        base.OnModelCreating(builder);

        builder.ConfigureSaas();
    }
}
