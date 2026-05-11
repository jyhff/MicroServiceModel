using System.Collections.Generic;

namespace LCH.Abp.BackgroundTasks;

public interface IJobDefinitionContext
{
    JobDefinition GetOrNull(string name);

    IReadOnlyList<JobDefinition> GetAll();

    void Add(params JobDefinition[] definitions);
}
