using Volo.Abp.Domain.Entities;

namespace LCH.Platform.Packages;

public class PackageUpdateDto : PackageCreateOrUpdateDto, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; }
}
