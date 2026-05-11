using Volo.Abp.Data;

namespace LCH.Abp.RulesEngineManagement;
public static class RulesEngineManagementDbPropertites
{
    public static string DbTablePrefix { get; set; } = AbpCommonDbProperties.DbTablePrefix + "RulesEngine";

    public static string DbSchema { get; set; } = AbpCommonDbProperties.DbSchema;


    public const string ConnectionStringName = "RulesEngineManagement";
}
