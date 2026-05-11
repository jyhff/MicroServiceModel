using System;
using Volo.Abp.EventBus;

namespace LCH.Abp.BackgroundTasks.EventBus;

[Serializable]
[EventName("abp.background-tasks.job.delete")]
public class JobDeleteEventData : JobEventData
{
}
