using System.Threading.Tasks;

namespace LCH.Abp.WeChat.Common.Messages.Handlers;
public interface IEventHandleContributor<TMessage> where TMessage : WeChatEventMessage
{
    Task HandleAsync(MessageHandleContext<TMessage> context);
}
