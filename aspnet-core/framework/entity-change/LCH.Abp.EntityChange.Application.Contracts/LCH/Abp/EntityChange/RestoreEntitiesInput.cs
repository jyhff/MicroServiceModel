using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.EntityChange;

public class RestoreEntitiesInput
{
    [Required]
    public List<RestoreEntityInput> Entities { get; set; }
}
