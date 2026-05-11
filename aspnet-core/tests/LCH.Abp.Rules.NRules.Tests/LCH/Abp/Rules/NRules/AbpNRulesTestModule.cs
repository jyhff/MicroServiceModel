using Volo.Abp.Modularity;

namespace LCH.Abp.Rules.NRules
{
    [DependsOn(
        typeof(AbpNRulesModule))]
    public class AbpNRulesTestModule : AbpModule
    {
    }
}
