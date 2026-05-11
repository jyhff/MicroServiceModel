using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace LCH.Abp.BackgroundTasks.Quartz;

[DisallowConcurrentExecution]
public class QuartzJobConcurrentAdapter<TJobRunnable> : QuartzJobSimpleAdapter<TJobRunnable>
    where TJobRunnable : IJobRunnable
{
    public QuartzJobConcurrentAdapter(IServiceScopeFactory serviceScopeFactory)
        : base(serviceScopeFactory)
    {
    }
}
