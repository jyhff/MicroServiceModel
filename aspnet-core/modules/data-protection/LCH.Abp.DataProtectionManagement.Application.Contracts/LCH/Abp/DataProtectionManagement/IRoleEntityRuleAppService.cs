using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace LCH.Abp.DataProtectionManagement;
public interface IRoleEntityRuleAppService : IApplicationService
{
    Task<RoleEntityRuleDto> GetAsync(RoleEntityRuleGetInput input);

    Task<RoleEntityRuleDto> CreateAsync(RoleEntityRuleCreateDto input);

    Task<RoleEntityRuleDto> UpdateAsync(Guid id, RoleEntityRuleUpdateDto input);
}
