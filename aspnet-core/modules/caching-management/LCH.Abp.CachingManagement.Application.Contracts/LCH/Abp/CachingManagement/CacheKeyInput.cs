using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.CachingManagement;

public class CacheKeyInput
{
    [Required]
    public string Key { get; set; }
}
