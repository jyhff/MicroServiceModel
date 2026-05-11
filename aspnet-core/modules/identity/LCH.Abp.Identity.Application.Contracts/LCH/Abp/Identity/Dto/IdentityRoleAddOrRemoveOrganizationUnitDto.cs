using System;
using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.Identity;

public class IdentityRoleAddOrRemoveOrganizationUnitDto
{
    [Required]
    public Guid[] OrganizationUnitIds { get; set; }
}
