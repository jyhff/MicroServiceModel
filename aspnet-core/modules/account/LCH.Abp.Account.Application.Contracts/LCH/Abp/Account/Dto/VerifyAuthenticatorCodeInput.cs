using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.Account;
public class VerifyAuthenticatorCodeInput
{
    [Required]
    [StringLength(6)]
    public string AuthenticatorCode { get; set; }
}
