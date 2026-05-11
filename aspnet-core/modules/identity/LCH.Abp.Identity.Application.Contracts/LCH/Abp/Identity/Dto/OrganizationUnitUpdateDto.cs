using Volo.Abp.ObjectExtending;

namespace LCH.Abp.Identity;

public class OrganizationUnitUpdateDto : ExtensibleObject
{
    public string DisplayName { get; set; }
}
