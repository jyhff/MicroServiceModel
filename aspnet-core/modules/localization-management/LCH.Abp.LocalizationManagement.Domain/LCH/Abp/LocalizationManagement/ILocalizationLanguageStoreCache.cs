using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Localization;

namespace LCH.Abp.LocalizationManagement;

public interface ILocalizationLanguageStoreCache
{
    Task<IReadOnlyList<LanguageInfo>> GetLanguagesAsync();
}
