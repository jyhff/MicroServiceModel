using LCH.Abp.Aliyun;
using LCH.Abp.Aliyun.Features;
using LCH.Abp.Tests.Features;
using Volo.Abp.Modularity;

namespace LCH.Abp.Sms.Aliyun
{
    [DependsOn(
        typeof(AbpAliyunTestModule),
        typeof(AbpAliyunSmsModule))]
    public class AbpAliyunSmsTestModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            Configure<FakeFeatureOptions>(options =>
            {
                options.Map(AliyunFeatureNames.Sms.Enable, (_) => "true");
            });
        }
    }
}
