using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.Identity;

public class OrganizationUnitAddUserDto
{
    [Required]
    public List<Guid> UserIds { get; set; }
}
