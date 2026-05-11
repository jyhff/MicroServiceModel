using Volo.Abp.Collections;

namespace LCH.Abp.Rules.NRules;

public class AbpNRulesOptions
{
    public ITypeList<RuleBase> DefinitionRules { get; }

    public AbpNRulesOptions()
    {
        DefinitionRules = new TypeList<RuleBase>();
    }
}
