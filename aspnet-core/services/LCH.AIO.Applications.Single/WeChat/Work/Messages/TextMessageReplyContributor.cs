using LCH.Abp.WeChat.Common.Messages.Handlers;
using LCH.Abp.WeChat.Work.Common.Messages.Models;
using LCH.Abp.WeChat.Work.Messages;

namespace LY.AIO.Applications.Single.WeChat.Work.Messages;
/// <summary>
/// 文本消息客服回复
/// </summary>
public class TextMessageReplyContributor : IMessageHandleContributor<TextMessage>
{
    public async virtual Task HandleAsync(MessageHandleContext<TextMessage> context)
    {
        var messageSender = context.ServiceProvider.GetRequiredService<IWeChatWorkMessageSender>();

        await messageSender.SendAsync(
            new LCH.Abp.WeChat.Work.Messages.Models.WeChatWorkTextMessage(
                context.Message.AgentId.ToString(),
                new LCH.Abp.WeChat.Work.Messages.Models.TextMessage(
                    context.Message.Content))
            {
                ToUser = context.Message.FromUserName,
            });
    }
}
