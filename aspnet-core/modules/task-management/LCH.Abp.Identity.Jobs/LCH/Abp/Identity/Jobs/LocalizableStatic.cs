using Volo.Abp.Identity.Localization;
using Volo.Abp.Localization;

namespace LCH.Abp.Identity.Jobs;

internal static class LocalizableStatic
{
    public static ILocalizableString Create(string name)
    {
        return LocalizableString.Create<IdentityResource>(name);
    }
}
