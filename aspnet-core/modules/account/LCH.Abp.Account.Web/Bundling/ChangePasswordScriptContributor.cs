using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Packages.JQuery;
using Volo.Abp.Modularity;

namespace LCH.Abp.Account.Web.Bundling;

[DependsOn(typeof(JQueryScriptContributor))]
public class ChangePasswordScriptContributor : BundleContributor
{
    public override Task ConfigureBundleAsync(BundleConfigurationContext context)
    {
        context.Files.Add("/Pages/Account/ChangePassword.js");

        return Task.CompletedTask;
    }
}
