using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.OpenIddict.Localization;

namespace LCH.Abp.OpenIddict;

public abstract class OpenIddictControllerBase : AbpControllerBase
{
    protected OpenIddictControllerBase()
    {
        LocalizationResource = typeof(AbpOpenIddictResource);
    }
}
