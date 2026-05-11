using LCH.Abp.BackgroundTasks;

namespace LCH.Abp.Saas.Jobs;
public class SaasJobDefinitionProvider : JobDefinitionProvider
{
    public override void Define(IJobDefinitionContext context)
    {
        context.Add(new JobDefinition(
            "TenantUsageMonitoringJob",
            typeof(TenantUsageMonitoringJob),
            LocalizableStatic.Create("TenantUsageMonitoringJob"),
            TenantUsageMonitoringJob.Paramters));
    }
}
