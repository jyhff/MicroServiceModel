using LCH.Abp.WeChat.Common.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace LCH.Abp.WeChat.Official.Messages;
public abstract class WeChatOfficialMessageResolveContributorBase : MessageResolveContributorBase
{
    public override Task ResolveAsync(IMessageResolveContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<AbpWeChatOfficialMessageResolveOptions>>().Value;

        return ResolveMessageAsync(context, options);
    }

    protected abstract Task ResolveMessageAsync(IMessageResolveContext context, AbpWeChatOfficialMessageResolveOptions options);
}
