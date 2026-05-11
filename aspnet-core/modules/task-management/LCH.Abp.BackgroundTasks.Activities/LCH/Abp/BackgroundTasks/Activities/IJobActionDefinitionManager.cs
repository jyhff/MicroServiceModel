using JetBrains.Annotations;
using System.Collections.Generic;

namespace LCH.Abp.BackgroundTasks.Activities;

public interface IJobActionDefinitionManager
{
    [NotNull]
    JobActionDefinition Get([NotNull] string name);

    IReadOnlyList<JobActionDefinition> GetAll();

    JobActionDefinition GetOrNull(string name);
}
