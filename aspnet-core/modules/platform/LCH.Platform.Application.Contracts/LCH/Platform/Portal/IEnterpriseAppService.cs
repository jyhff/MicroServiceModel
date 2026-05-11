using System;
using Volo.Abp.Application.Services;

namespace LCH.Platform.Portal;

public interface IEnterpriseAppService :
    ICrudAppService<
        EnterpriseDto,
        Guid,
        EnterpriseGetListInput,
        EnterpriseCreateDto,
        EnterpriseUpdateDto>
{
}
