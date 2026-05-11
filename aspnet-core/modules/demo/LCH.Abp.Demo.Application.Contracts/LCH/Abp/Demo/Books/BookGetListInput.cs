using Volo.Abp.Application.Dtos;

namespace LCH.Abp.Demo.Books;

public class BookGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
