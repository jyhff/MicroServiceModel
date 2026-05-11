using System.Threading;
using Volo.Abp.BackgroundWorkers;

namespace LCH.Abp.BackgroundTasks;

public interface IBackgroundWorkerRunnable : IJobRunnable
{
#nullable enable
    JobInfo? BuildWorker(IBackgroundWorker worker);
#nullable disable
}
