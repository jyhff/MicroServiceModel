using LCH.Abp.Account.Web.Pages.Account.Components.ProfileManagementGroup.Authenticator;
using LCH.Abp.Account.Web.Pages.Account.Components.ProfileManagementGroup.SecurityLog;
using LCH.Abp.Account.Web.Pages.Account.Components.ProfileManagementGroup.Session;
using LCH.Abp.Account.Web.Pages.Account.Components.ProfileManagementGroup.TwoFactor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using Volo.Abp.Account.Localization;
using Volo.Abp.Account.Web.ProfileManagement;

namespace LCH.Abp.Account.Web.ProfileManagement;

public class ProfileManagementPageContributor : IProfileManagementPageContributor
{
    public virtual Task ConfigureAsync(ProfileManagementPageCreationContext context)
    {
        var l = context.ServiceProvider.GetRequiredService<IStringLocalizer<AccountResource>>();

        context.Groups.Add(
            new ProfileManagementPageGroup(
                "LCH.Abp.Account.Session",
                l["ProfileTab:Session"],
                typeof(AccountProfileSessionManagementGroupViewComponent)
            )
        );

        context.Groups.Add(
            new ProfileManagementPageGroup(
                "LCH.Abp.Account.TwoFactor",
                l["ProfileTab:TwoFactor"],
                typeof(AccountProfileTwoFactorManagementGroupViewComponent)
            )
        );

        context.Groups.Add(
            new ProfileManagementPageGroup(
                "LCH.Abp.Account.SecurityLog",
                l["ProfileTab:SecurityLog"],
                typeof(AccountProfileSecurityLogManagementGroupViewComponent)
            )
        );

        context.Groups.Add(
            new ProfileManagementPageGroup(
                "LCH.Abp.Account.Authenticator",
                l["ProfileTab:Authenticator"],
                typeof(AccountProfileAuthenticatorManagementGroupViewComponent)
            )
        );

        return Task.CompletedTask;
    }
}
