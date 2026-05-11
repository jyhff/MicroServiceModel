using System;
using Volo.Abp.Application.Dtos;

namespace LCH.Abp.OpenIddict.Scopes;

[Serializable]
public class OpenIddictScopeGetListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
