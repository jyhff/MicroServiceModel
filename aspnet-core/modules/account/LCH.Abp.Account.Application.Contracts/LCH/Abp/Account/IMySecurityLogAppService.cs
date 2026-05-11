using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace LCH.Abp.Account;

public interface IMySecurityLogAppService
{
    Task<PagedResultDto<SecurityLogDto>> GetListAsync(SecurityLogGetListInput input);

    Task<SecurityLogDto> GetAsync(Guid id);

    Task DeleteAsync(Guid id);
}
