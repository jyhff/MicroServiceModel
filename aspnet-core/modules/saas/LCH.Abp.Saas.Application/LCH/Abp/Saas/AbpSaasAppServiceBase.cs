using LCH.Abp.Saas.Localization;
using Volo.Abp.Application.Services;

namespace LCH.Abp.Saas;
public abstract class AbpSaasAppServiceBase : ApplicationService
{
    protected AbpSaasAppServiceBase()
    {
        ObjectMapperContext = typeof(AbpSaasApplicationModule);
        LocalizationResource = typeof(AbpSaasResource);
    }
}
