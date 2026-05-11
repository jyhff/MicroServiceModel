using LCH.Abp.WeChat.Official;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace LCH.Abp.Authentication.WeChat;

[DependsOn(typeof(AbpWeChatOfficialModule))]
public class AbpAuthenticationWeChatModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services
            .AddAuthentication()
            .AddWeChat();
    }
}