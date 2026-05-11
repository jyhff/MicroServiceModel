using System.ComponentModel.DataAnnotations;
using Volo.Abp.Identity;
using Volo.Abp.Validation;

namespace LCH.Abp.Account;
public class SendEmailSigninCodeDto
{
    [Required]
    [EmailAddress]
    [Display(Name = "EmailAddress")]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxEmailLength))]
    public string EmailAddress { get; set; }
}
