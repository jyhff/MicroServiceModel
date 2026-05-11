using LCH.Abp.IdGenerator.Snowflake;

namespace LCH.Abp.Serilog.Enrichers.UniqueId;

public class AbpSerilogEnrichersUniqueIdOptions
{
    public SnowflakeIdOptions SnowflakeIdOptions { get; set; }
    public AbpSerilogEnrichersUniqueIdOptions()
    {
        SnowflakeIdOptions = new SnowflakeIdOptions();
    }
}
