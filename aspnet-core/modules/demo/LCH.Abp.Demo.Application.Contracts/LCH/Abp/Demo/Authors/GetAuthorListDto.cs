using Volo.Abp.Application.Dtos;

namespace LCH.Abp.Demo.Authors;
public class GetAuthorListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}