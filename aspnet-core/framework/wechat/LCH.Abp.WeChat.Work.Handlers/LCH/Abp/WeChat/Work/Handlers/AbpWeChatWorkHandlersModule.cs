using LCH.Abp.WeChat.Work.Common;
using LCH.Abp.WeChat.Work.Contacts;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace LCH.Abp.WeChat.Work.Handlers;

[DependsOn(typeof(AbpEventBusModule))]
[DependsOn(typeof(AbpWeChatWorkCommonModule))]
[DependsOn(typeof(AbpWeChatWorkContactModule))]
public class AbpWeChatWorkHandlersModule : AbpModule
{
}
