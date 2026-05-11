using Elsa.Activities.Console;
using Elsa.Builders;
using LCH.Abp.Elsa.Notifications;

namespace LCH.Abp.Elsa.Activities.Webhooks.Tests;

public class PublishWebhookWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder
            .WithCompletedNotification("Test-Notification")
            .WriteLine("This a simple workflow with publish webhook.")
            .PublishWebhook(
                setup: activity => 
                    activity.WithWebhooName(PublishWebhookData.Name)
                            .WithWebhookData(PublishWebhookData.SendData)
                            .WithSendExactSameData(true)
                            .WithUseOnlyGivenHeaders(false)
                            .WithHeaders(new Dictionary<string, string>
                            {
                                { "X-With", "Test" }
                            })
                            .WithTenantId(PublishWebhookData.TenantId))
            .WriteLine("Workflow finished.");
    }
}
