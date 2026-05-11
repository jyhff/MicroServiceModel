using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace LCH.Abp.WeChat.Official;

[DependsOn(
    typeof(AbpDddApplicationContractsModule))]
public class AbpWeChatOfficialApplicationContractsModule : AbpModule
{
}
