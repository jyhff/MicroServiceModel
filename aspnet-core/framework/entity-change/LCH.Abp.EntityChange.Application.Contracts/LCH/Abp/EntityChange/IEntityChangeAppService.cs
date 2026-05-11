using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LCH.Abp.EntityChange;

public interface IEntityChangeAppService : IApplicationService
{
    Task<PagedResultDto<EntityChangeDto>> GetListAsync(EntityChangeGetListInput input);
}
