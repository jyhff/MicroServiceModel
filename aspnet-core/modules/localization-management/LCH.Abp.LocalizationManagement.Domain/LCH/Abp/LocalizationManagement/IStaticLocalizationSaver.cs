using System.Threading.Tasks;

namespace LCH.Abp.LocalizationManagement;

public interface IStaticLocalizationSaver
{
    Task SaveAsync();
}
