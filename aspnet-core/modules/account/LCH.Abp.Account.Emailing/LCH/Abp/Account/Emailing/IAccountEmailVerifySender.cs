using System;
using System.Threading.Tasks;

namespace LCH.Abp.Account.Emailing;

[Obsolete("This interface has been deprecated. Please use LCH.Abp.Account.Security.IAccountEmailSecurityCodeSender.")]
public interface IAccountEmailVerifySender
{
    Task SendMailLoginVerifyCodeAsync(
        string code,
        string userName,
        string emailAddress);
}
