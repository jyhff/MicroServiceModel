using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace LCH.Abp.SettingManagement;

public interface IReadonlySettingAppService : IApplicationService
{
    Task<SettingGroupResult> GetAllForGlobalAsync();

    Task<SettingGroupResult> GetAllForCurrentTenantAsync();
}
