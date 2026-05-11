using LCH.Platform.HttpApi.Client;
using Volo.Abp.Emailing;
using Volo.Abp.Modularity;

namespace LCH.Abp.Emailing.Platform;

[DependsOn(
    typeof(AbpEmailingModule),
    typeof(PlatformHttpApiClientModule))]
public class AbpEmailingPlatformModule : AbpModule
{

}
