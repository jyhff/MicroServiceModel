using Volo.Abp.Domain.Entities;

namespace LCH.Abp.TaskManagement;

public class BackgroundJobInfoUpdateDto : BackgroundJobInfoCreateOrUpdateDto, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; }
}
