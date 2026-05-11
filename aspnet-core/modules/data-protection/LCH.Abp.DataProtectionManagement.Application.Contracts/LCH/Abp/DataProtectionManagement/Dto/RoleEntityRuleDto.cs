using System;

namespace LCH.Abp.DataProtectionManagement;

public class RoleEntityRuleDto : EntityRuleDtoBase
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; }
}
