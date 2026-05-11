using LCH.Platform.Localization;
using Volo.Abp.Application.Services;

namespace LCH.Platform;

public abstract class PlatformApplicationServiceBase : ApplicationService
{
    protected PlatformApplicationServiceBase()
    {
        LocalizationResource = typeof(PlatformResource);
        ObjectMapperContext = typeof(PlatformApplicationModule);
    }
}
