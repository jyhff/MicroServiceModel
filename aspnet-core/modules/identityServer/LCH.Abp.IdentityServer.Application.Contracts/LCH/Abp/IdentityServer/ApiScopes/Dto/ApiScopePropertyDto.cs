using Volo.Abp.Application.Dtos;

namespace LCH.Abp.IdentityServer.ApiScopes;

public class ApiScopePropertyDto : EntityDto
{
    public string Key { get; set; }

    public string Value { get; set; }
}
