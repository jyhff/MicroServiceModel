using Volo.Abp.Domain.Entities;

namespace LCH.Platform.Portal;

public class EnterpriseUpdateDto : EnterpriseCreateOrUpdateDto, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; }
}
