using LCH.Abp.WebhooksManagement.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;

namespace LCH.Abp.WebhooksManagement.Features;

public class WebhooksManagementFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        //var group = context.AddGroup(WebhooksManagementFeatureNames.GroupName, L("Features:WebhooksManagement"));
    }

    private static ILocalizableString L(string name)
    {
        return LocalizableString.Create<WebhooksManagementResource>(name);
    }
}
