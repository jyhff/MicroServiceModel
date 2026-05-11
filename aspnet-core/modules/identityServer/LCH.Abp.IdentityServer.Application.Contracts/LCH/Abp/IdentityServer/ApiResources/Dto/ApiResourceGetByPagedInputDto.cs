using Volo.Abp.Application.Dtos;

namespace LCH.Abp.IdentityServer.ApiResources;

public class ApiResourceGetByPagedInputDto : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
