using System;
using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.Account;

public class GetTwoFactorProvidersInput
{
    [Required]
    public Guid UserId { get; set; }
}
