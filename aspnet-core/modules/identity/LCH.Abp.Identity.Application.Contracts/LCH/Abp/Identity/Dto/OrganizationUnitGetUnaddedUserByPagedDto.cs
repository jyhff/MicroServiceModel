using Volo.Abp.Application.Dtos;

namespace LCH.Abp.Identity;

public class OrganizationUnitGetUnaddedUserByPagedDto : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
