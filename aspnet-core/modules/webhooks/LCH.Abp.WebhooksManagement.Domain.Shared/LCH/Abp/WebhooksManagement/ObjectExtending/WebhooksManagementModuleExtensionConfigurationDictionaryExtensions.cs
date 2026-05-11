using System;
using Volo.Abp.ObjectExtending.Modularity;

namespace LCH.Abp.WebhooksManagement.ObjectExtending;

public static class WebhooksManagementModuleExtensionConfigurationDictionaryExtensions
{
    public static ModuleExtensionConfigurationDictionary ConfigureWebhooksManagement(
        this ModuleExtensionConfigurationDictionary modules,
        Action<WebhooksManagementModuleExtensionConfiguration> configureAction)
    {
        return modules.ConfigureModule(
            WebhooksManagementModuleExtensionConsts.ModuleName,
            configureAction
        );
    }
}
