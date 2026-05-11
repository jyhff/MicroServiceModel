using NRules.Fluent.Dsl;
using Volo.Abp.DependencyInjection;

namespace LCH.Abp.Rules.NRules;

public abstract class RuleBase : Rule, ITransientDependency
{
}
