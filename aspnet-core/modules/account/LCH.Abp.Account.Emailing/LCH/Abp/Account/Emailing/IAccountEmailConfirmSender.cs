using System;
using System.Threading.Tasks;

namespace LCH.Abp.Account.Emailing;

[Obsolete("This interface has been deprecated. Please use LCH.Abp.Account.Security.IAccountEmailSecurityCodeSender.")]
public interface IAccountEmailConfirmSender
{
    Task SendEmailConfirmLinkAsync(
        Guid userId,
        string userEmail,
        string confirmToken,
        string appName,
        string returnUrl = null,
        string returnUrlHash = null,
        Guid? userTenantId = null
    );
}
