using System.ComponentModel.DataAnnotations;
using Volo.Abp.Validation;

namespace LCH.Platform.Packages;

public class PackageGetLatestInput
{
    [Required]
    [DynamicMaxLength(typeof(PackageConsts), nameof(PackageConsts.MaxNameLength))]
    public string Name { get; set; }

    [DynamicMaxLength(typeof(PackageConsts), nameof(PackageConsts.MaxVersionLength))]
    public string Version { get; set; }
}
