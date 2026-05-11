using LCH.Platform.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Settings;

namespace LCH.Platform;

public abstract class PlatformControllerBase : AbpControllerBase
{
    protected ISettingProvider SettingProvider => LazyServiceProvider.LazyGetRequiredService<ISettingProvider>();

    protected PlatformControllerBase()
    {
        LocalizationResource = typeof(PlatformResource);
    }
}
