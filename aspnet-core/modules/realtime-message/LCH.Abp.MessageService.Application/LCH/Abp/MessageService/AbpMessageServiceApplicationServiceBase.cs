using LCH.Abp.MessageService.Localization;
using Volo.Abp.Application.Services;

namespace LCH.Abp.MessageService;

public abstract class AbpMessageServiceApplicationServiceBase : ApplicationService
{
    protected AbpMessageServiceApplicationServiceBase()
    {
        LocalizationResource = typeof(MessageServiceResource);
        ObjectMapperContext = typeof(AbpMessageServiceApplicationModule);
    }
}
