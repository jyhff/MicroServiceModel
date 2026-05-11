using System.Threading.Tasks;

namespace LCH.Abp.SettingManagement;

public interface ISettingAppService : IReadonlySettingAppService
{
    Task SetGlobalAsync(UpdateSettingsDto input);

    Task SetCurrentTenantAsync(UpdateSettingsDto input);
}
