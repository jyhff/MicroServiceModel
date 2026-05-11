using LCH.Platform.HttpApi.Client;
using Volo.Abp.Modularity;
using Volo.Abp.Sms;

namespace LCH.Abp.Sms.Platform;

[DependsOn(
    typeof(AbpSmsModule),
    typeof(PlatformHttpApiClientModule))]
public class AbpSmsPlatformModule : AbpModule
{

}
