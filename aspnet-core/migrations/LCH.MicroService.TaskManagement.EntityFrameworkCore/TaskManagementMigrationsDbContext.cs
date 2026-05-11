using LCH.Abp.TaskManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace LCH.MicroService.TaskManagement.EntityFrameworkCore;

[ConnectionStringName("TaskManagementDbMigrator")]
public class TaskManagementMigrationsDbContext : AbpDbContext<TaskManagementMigrationsDbContext>
{
    public TaskManagementMigrationsDbContext(DbContextOptions<TaskManagementMigrationsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureTaskManagement();
    }
}
