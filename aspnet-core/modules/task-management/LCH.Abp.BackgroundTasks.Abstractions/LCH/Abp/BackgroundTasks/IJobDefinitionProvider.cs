namespace LCH.Abp.BackgroundTasks;

public interface IJobDefinitionProvider
{
    void Define(IJobDefinitionContext context);
}
