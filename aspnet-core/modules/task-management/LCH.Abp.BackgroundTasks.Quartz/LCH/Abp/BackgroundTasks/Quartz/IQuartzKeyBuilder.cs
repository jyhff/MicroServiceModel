using Quartz;

namespace LCH.Abp.BackgroundTasks.Quartz;

public interface IQuartzKeyBuilder
{
    JobKey CreateJobKey(JobInfo jobInfo);

    TriggerKey CreateTriggerKey(JobInfo jobInfo);
}
