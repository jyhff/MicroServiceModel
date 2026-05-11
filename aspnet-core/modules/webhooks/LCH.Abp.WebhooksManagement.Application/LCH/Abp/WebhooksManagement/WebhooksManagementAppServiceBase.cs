using LCH.Abp.WebhooksManagement.Localization;
using Volo.Abp.Application.Services;

namespace LCH.Abp.WebhooksManagement;

public abstract class WebhooksManagementAppServiceBase : ApplicationService
{
    protected WebhooksManagementAppServiceBase()
    {
        LocalizationResource = typeof(WebhooksManagementResource);
        ObjectMapperContext = typeof(WebhooksManagementApplicationModule);
    }
}
