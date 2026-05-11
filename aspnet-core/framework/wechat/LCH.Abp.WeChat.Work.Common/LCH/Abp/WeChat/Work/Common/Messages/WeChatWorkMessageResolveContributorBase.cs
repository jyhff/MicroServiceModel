using LCH.Abp.WeChat.Common.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace LCH.Abp.WeChat.Work.Common.Messages;
public abstract class WeChatWorkMessageResolveContributorBase : MessageResolveContributorBase
{
    public override Task ResolveAsync(IMessageResolveContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<AbpWeChatWorkMessageResolveOptions>>().Value;

        return ResolveMessageAsync(context, options);
    }

    protected abstract Task ResolveMessageAsync(IMessageResolveContext context, AbpWeChatWorkMessageResolveOptions options);
}
