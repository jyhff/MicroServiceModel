using System;
using Volo.Abp.Application.Services;

namespace LCH.Abp.IdentityServer.ApiScopes;

public interface IApiScopeAppService : 
    ICrudAppService<
        ApiScopeDto,
        Guid,
        GetApiScopeInput,
        ApiScopeCreateDto,
        ApiScopeUpdateDto>
{
}
