using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.Account;

public class ConfirmEmailInput
{
    [Required]
    public string ConfirmToken { get; set; }
}
