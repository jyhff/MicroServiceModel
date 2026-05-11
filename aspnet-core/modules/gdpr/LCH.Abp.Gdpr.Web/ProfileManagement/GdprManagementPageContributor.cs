using LCH.Abp.Gdpr.Localization;
using LCH.Abp.Gdpr.Web.Pages.Account.Components.ProfileManagementGroup.Gdpr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using Volo.Abp.Account.Web.ProfileManagement;

namespace LCH.Abp.Gdpr.Web.ProfileManagement;

public class GdprManagementPageContributor : IProfileManagementPageContributor
{
    public virtual Task ConfigureAsync(ProfileManagementPageCreationContext context)
    {
        var l = context.ServiceProvider.GetRequiredService<IStringLocalizer<GdprResource>>();

        context.Groups.Add(
            new ProfileManagementPageGroup(
                "LCH.Abp.Account.Gdpr",
                l["PersonalData"],
                typeof(AccountProfileGdprManagementGroupViewComponent)
            )
        );

        return Task.CompletedTask;
    }
}
