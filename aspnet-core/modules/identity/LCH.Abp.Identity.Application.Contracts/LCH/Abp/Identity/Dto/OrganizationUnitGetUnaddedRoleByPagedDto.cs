using Volo.Abp.Application.Dtos;

namespace LCH.Abp.Identity;

public class OrganizationUnitGetUnaddedRoleByPagedDto : PagedAndSortedResultRequestDto
{

    public string Filter { get; set; }
}
