using System.Collections.Generic;

namespace LCH.Abp.Account;
public class AuthenticatorRecoveryCodeDto
{
    public List<string> RecoveryCodes { get; set; }
}
