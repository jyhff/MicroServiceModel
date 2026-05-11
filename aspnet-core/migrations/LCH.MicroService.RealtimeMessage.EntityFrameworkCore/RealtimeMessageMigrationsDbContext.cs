using LCH.Abp.MessageService.EntityFrameworkCore;
using LCH.Abp.Notifications.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace LCH.MicroService.RealtimeMessage.EntityFrameworkCore;

[ConnectionStringName("RealtimeMessageDbMigrator")]
public class RealtimeMessageMigrationsDbContext : AbpDbContext<RealtimeMessageMigrationsDbContext>
{
    public RealtimeMessageMigrationsDbContext(DbContextOptions<RealtimeMessageMigrationsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureNotifications();
        modelBuilder.ConfigureNotificationsDefinition();
        modelBuilder.ConfigureMessageService();
    }
}
