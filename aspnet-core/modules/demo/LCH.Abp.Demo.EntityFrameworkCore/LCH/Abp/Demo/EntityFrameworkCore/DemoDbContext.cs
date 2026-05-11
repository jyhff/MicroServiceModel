using LCH.Abp.DataProtection.EntityFrameworkCore;
using LCH.Abp.Demo.Authors;
using LCH.Abp.Demo.Books;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;

namespace LCH.Abp.Demo.EntityFrameworkCore;

[ConnectionStringName(DemoDbProterties.ConnectionStringName)]
public class DemoDbContext : AbpDataProtectionDbContext<DemoDbContext>, IDemoDbContext
{
    public DbSet<Book> Books { get; set; }

    public DbSet<Author> Authors { get; set; }


    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureDemo(); ;
    }
}
