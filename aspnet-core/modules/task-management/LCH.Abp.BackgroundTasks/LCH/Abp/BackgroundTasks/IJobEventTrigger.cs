using System.Threading.Tasks;

namespace LCH.Abp.BackgroundTasks;

public interface IJobEventTrigger
{
    Task OnJobBeforeExecuted(JobEventContext context);

    Task OnJobAfterExecuted(JobEventContext context);
}
