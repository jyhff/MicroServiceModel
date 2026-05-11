using LCH.Abp.WebhooksManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace LCH.Abp.WebhooksManagement;

public abstract class WebhooksManagementControllerBase : AbpControllerBase
{
    protected WebhooksManagementControllerBase()
    {
        LocalizationResource = typeof(WebhooksManagementResource);
    }
}
