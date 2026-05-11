using LCH.Abp.Tests.Features;
using LCH.Abp.WeChat.Work.Contacts.Features;
using Volo.Abp.Modularity;

namespace LCH.Abp.WeChat.Work.Contacts;

[DependsOn(
    typeof(AbpWeChatWorkContactModule),
    typeof(AbpWeChatWorkTestModule))]
public class AbpWeChatWorkContactTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FakeFeatureOptions>(options =>
        {
            options.Map(WeChatWorkContactsFeatureNames.Enable, (feature) =>
            {
                return true.ToString();
            });
        });
    }
}
