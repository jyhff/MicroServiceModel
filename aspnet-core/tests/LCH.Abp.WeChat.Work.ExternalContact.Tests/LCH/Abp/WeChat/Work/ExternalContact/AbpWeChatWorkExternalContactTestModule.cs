using LCH.Abp.Tests.Features;
using LCH.Abp.WeChat.Work.ExternalContact.Features;
using Volo.Abp.Modularity;

namespace LCH.Abp.WeChat.Work.ExternalContact;

[DependsOn(
    typeof(AbpWeChatWorkExternalContactModule),
    typeof(AbpWeChatWorkTestModule))]
public class AbpWeChatWorkExternalContactTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FakeFeatureOptions>(options =>
        {
            options.Map(WeChatWorkExternalContactFeatureNames.Enable, (feature) =>
            {
                return true.ToString();
            });
        });
    }
}
