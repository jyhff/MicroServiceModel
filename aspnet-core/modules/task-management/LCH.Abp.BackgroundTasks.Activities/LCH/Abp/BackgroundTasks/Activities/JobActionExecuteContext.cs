using JetBrains.Annotations;

namespace LCH.Abp.BackgroundTasks.Activities;

public class JobActionExecuteContext
{
    public JobAction Action { get; }
    public JobEventContext Event { get; }
    public JobActionExecuteContext(
        [NotNull] JobAction action,
        [NotNull] JobEventContext @event)
    {
        Action = action;
        Event = @event;
    }
}
