using Volo.Abp.Application.Dtos;

namespace LCH.Abp.OssManagement;

public class GetOssContainersInput : PagedAndSortedResultRequestDto
{
    public string Prefix { get; set; }
    public string Marker { get; set; }
}
