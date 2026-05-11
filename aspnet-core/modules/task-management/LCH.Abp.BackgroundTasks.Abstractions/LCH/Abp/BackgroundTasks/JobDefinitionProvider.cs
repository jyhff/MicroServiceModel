using Volo.Abp.DependencyInjection;

namespace LCH.Abp.BackgroundTasks;

public abstract class JobDefinitionProvider : IJobDefinitionProvider, ITransientDependency
{
    public abstract void Define(IJobDefinitionContext context);
}
