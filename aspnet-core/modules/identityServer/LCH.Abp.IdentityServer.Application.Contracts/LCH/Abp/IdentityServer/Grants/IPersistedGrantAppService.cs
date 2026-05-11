using System;
using Volo.Abp.Application.Services;

namespace LCH.Abp.IdentityServer.Grants;

public interface IPersistedGrantAppService : 
    IReadOnlyAppService<PersistedGrantDto, Guid, GetPersistedGrantInput>,
    IDeleteAppService<Guid>
{

}
