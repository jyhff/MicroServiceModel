using Volo.Abp.Application.Dtos;

namespace LCH.Abp.MessageService.Groups;

public class GroupSearchInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
