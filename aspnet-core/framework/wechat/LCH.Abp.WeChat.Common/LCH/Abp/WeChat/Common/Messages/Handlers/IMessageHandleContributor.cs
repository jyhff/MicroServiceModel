using System.Threading.Tasks;

namespace LCH.Abp.WeChat.Common.Messages.Handlers;
public interface IMessageHandleContributor<TMessage> where TMessage : WeChatGeneralMessage
{
    Task HandleAsync(MessageHandleContext<TMessage> context);
}
