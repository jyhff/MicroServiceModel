using Microsoft.Extensions.Configuration;
using Volo.Abp.Timing;

namespace LCH.MicroService.Applications.Single.DbMigrator;
public partial class SingleDbMigratorModule
{
    private void ConfigureTiming(IConfiguration configuration)
    {
        Configure<AbpClockOptions>(options =>
        {
            configuration.GetSection("Clock").Bind(options);
        });
    }
}
