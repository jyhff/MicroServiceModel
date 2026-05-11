using Volo.Abp.Application.Dtos;

namespace LCH.Abp.Saas.Tenants;

public class TenantGetListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}