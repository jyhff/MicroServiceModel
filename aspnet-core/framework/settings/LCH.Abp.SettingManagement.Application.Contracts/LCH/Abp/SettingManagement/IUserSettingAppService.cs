using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace LCH.Abp.SettingManagement;

public interface IUserSettingAppService : IApplicationService
{
    Task SetCurrentUserAsync(UpdateSettingsDto input);

    Task<SettingGroupResult> GetAllForCurrentUserAsync();
}
