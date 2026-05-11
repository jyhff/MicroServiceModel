using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.DataProtection.Models;

public class EntityTypeInfoGetModel
{
    [Required]
    public DataAccessOperation Operation { get; set; }
}
