using System;

namespace LCH.Abp.BackgroundTasks;
public interface IJobExceptionTypeFinder
{
    JobExceptionType GetExceptionType(JobEventContext eventContext, Exception exception);
}

