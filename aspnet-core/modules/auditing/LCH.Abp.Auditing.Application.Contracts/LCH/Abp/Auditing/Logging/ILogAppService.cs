using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LCH.Abp.Auditing.Logging;

public interface ILogAppService : IApplicationService
{
    Task<LogDto> GetAsync(string id);

    Task<PagedResultDto<LogDto>> GetListAsync(LogGetByPagedDto input);
}
