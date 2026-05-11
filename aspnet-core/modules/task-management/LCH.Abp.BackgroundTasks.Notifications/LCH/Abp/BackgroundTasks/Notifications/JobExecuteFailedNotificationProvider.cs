using JetBrains.Annotations;
using LCH.Abp.BackgroundTasks.Activities;
using LCH.Abp.BackgroundTasks.Localization;
using LCH.Abp.Notifications;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TextTemplating;

namespace LCH.Abp.BackgroundTasks.Notifications;

public class JobExecuteFailedNotificationProvider : NotificationJobExecutedProvider
{
    public const string Name = "JobExecutedFailedNofiter";
    public override string DefaultNotificationName => BackgroundTasksNotificationNames.JobExecuteFailed;

    public JobExecuteFailedNotificationProvider(
        ICurrentTenant currentTenant, 
        INotificationSender notificationSender, 
        ITemplateRenderer templateRenderer, 
        IStringLocalizer<BackgroundTasksResource> stringLocalizer) 
        : base(currentTenant, notificationSender, templateRenderer, stringLocalizer)
    {
    }

    public async override Task NotifyErrorAsync([NotNull] JobActionExecuteContext context)
    {
        var title = StringLocalizer["Notifications:JobExecuteFailed"].Value;

        await SendNofiterAsync(context, title, NotificationSeverity.Error);
    }
}
