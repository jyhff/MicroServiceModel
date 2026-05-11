using LCH.Abp.Rules.RulesEngine.FileProviders.Physical;
using LCH.Abp.Tests;
using System.IO;
using Volo.Abp.Modularity;

namespace LCH.Abp.Rules.RulesEngine
{
    [DependsOn(
        typeof(AbpRulesEngineModule),
        typeof(AbpTestsBaseModule))]
    public class AbpRulesEngineTestModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpRulesEnginePhysicalFileResolveOptions>(options =>
            {
                options.PhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), "Rules");
            });
        }
    }
}
