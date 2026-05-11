using Volo.Abp.Application.Dtos;

namespace LCH.Platform.Datas;

public class GetDataListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
