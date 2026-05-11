using LCH.Abp.Saas.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace LCH.Abp.Saas;
public abstract class AbpSaasControllerBase : AbpControllerBase
{
    protected AbpSaasControllerBase()
    {
        LocalizationResource = typeof(AbpSaasResource);
    }
}
