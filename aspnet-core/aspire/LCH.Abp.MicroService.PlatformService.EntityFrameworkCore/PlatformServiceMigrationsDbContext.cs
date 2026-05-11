using LCH.Platform.Datas;
using LCH.Platform.EntityFrameworkCore;
using LCH.Platform.Layouts;
using LCH.Platform.Menus;
using LCH.Platform.Messages;
using LCH.Platform.Packages;
using LCH.Platform.Portal;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace LCH.Abp.MicroService.PlatformService;

[ConnectionStringName("Default")]
public class PlatformServiceMigrationsDbContext : 
    AbpDbContext<PlatformServiceMigrationsDbContext>,
    IPlatformDbContext
{
    public DbSet<Menu> Menus { get; set; }

    public DbSet<Layout> Layouts { get; set; }

    public DbSet<RoleMenu> RoleMenus { get; set; }

    public DbSet<UserMenu> UserMenus { get; set; }

    public DbSet<UserFavoriteMenu> UserFavoriteMenus { get; set; }

    public DbSet<Platform.Datas.Data> Datas { get; set; }

    public DbSet<DataItem> DataItems { get; set; }

    public DbSet<Package> Packages { get; set; }

    public DbSet<PackageBlob> PackageBlobs { get; set; }

    public DbSet<Enterprise> Enterprises { get; set; }

    public DbSet<EmailMessage> EmailMessages { get; set; }

    public DbSet<SmsMessage> SmsMessages { get; set; }

    public PlatformServiceMigrationsDbContext(DbContextOptions<PlatformServiceMigrationsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigurePlatform();
    }
}
