using System.Collections.Generic;

namespace LCH.Abp.BackgroundTasks.Activities;

public interface IJobActionDefinitionContext
{
    JobActionDefinition GetOrNull(string name);

    IReadOnlyList<JobActionDefinition> GetAll();

    void Add(params JobActionDefinition[] definitions);
}
