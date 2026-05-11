using System;
using System.Collections.Generic;
using System.Text;

namespace LCH.Abp.WeChat.Work.OA.Schedules.Models;

public class CreateSchedule : CreateOrUpdateSchedule
{
    public CreateSchedule(DateTime startTime, DateTime endTime) 
        : base(startTime, endTime)
    {
    }
}
