using System;
using Volo.Abp.EventBus;

namespace LCH.Abp.BackgroundTasks.EventBus;

[Serializable]
[EventName("abp.background-tasks.job.pause")]
public class JobPauseEventData : JobEventData
{
}
