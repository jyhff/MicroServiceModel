using LCH.Abp.BackgroundTasks.Localization;
using Volo.Abp.Localization;
using Volo.Abp.TextTemplating;

namespace LCH.Abp.BackgroundTasks.Notifications.Templates;
public class BackgroundTasksNotificationTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(GetTemplateDefinitions());
    }

    private static TemplateDefinition[] GetTemplateDefinitions()
    {
        return new[]
        {
            new TemplateDefinition(
                BackgroundTasksNotificationTemplates.JobExecutedNotification,
                displayName: L("TextTemplate:JobExecutedNotification"),
                localizationResource: typeof(BackgroundTasksResource)
            ).WithVirtualFilePath(
                "/LCH/Abp/BackgroundTasks/Notifications/Templates/JobExecutedNotification.tpl",
                isInlineLocalized: true)
            };
    }

    private static ILocalizableString L(string name)
    {
        return LocalizableString.Create<BackgroundTasksResource>(name);
    }
}
