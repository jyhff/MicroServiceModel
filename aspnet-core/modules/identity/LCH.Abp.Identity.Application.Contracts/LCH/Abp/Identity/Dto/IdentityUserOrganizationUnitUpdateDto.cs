using System;
using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.Identity;

public class IdentityUserOrganizationUnitUpdateDto
{
    [Required]
    public Guid[] OrganizationUnitIds { get; set; }
}
