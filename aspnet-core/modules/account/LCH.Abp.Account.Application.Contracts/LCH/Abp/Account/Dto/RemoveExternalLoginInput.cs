using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.Account;
public class RemoveExternalLoginInput
{
    [Required]
    public string LoginProvider { get; set; }

    [Required]
    public string ProviderKey { get; set; }
}
