using Volo.Abp.Application.Dtos;

namespace LCH.Abp.IdentityServer.ApiScopes;

public class GetApiScopeInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
