using LCH.Abp.Tests.Features;
using LCH.Abp.WeChat.Work.OA.Features;
using Volo.Abp.Modularity;

namespace LCH.Abp.WeChat.Work.OA;

[DependsOn(
    typeof(AbpWeChatWorkOAModule),
    typeof(AbpWeChatWorkTestModule))]
public class AbpWeChatWorkOATestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FakeFeatureOptions>(options =>
        {
            options.Map(WeChatWorkOAFeatureNames.Enable, (feature) =>
            {
                return true.ToString();
            });
        });
    }
}
