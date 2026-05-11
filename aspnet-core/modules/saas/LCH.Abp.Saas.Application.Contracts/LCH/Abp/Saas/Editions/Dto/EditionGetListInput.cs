using Volo.Abp.Application.Dtos;

namespace LCH.Abp.Saas.Editions;

public class EditionGetListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
