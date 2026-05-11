using Volo.Abp.AspNetCore.Mvc.UI.Packages.JQuery;

namespace LCH.MicroService.Applications.Single.Bundling;

[DependsOn(typeof(JQueryScriptContributor))]
public class SingleGlobalScriptContributor : BundleContributor
{
    public override Task ConfigureBundleAsync(BundleConfigurationContext context)
    {
        context.Files.Add("/scripts/abp-wrapper.js");

        return Task.CompletedTask;
    }
}