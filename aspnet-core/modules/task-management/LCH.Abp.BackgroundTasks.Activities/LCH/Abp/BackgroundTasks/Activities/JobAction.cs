using System.Collections.Generic;

namespace LCH.Abp.BackgroundTasks.Activities;

public class JobAction
{
    public string Name { get; set; }
    public Dictionary<string, object> Paramters { get; set; }
}
