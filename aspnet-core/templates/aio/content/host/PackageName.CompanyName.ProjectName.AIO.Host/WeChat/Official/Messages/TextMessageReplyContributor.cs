using LCH.Abp.WeChat.Common.Messages.Handlers;
using LCH.Abp.WeChat.Official.Messages.Models;
using LCH.Abp.WeChat.Official.Services;

namespace PackageName.CompanyName.ProjectName.AIO.Host.WeChat.Official.Messages;
/// <summary>
/// 文本消息客服回复
/// </summary>
public class TextMessageReplyContributor : IMessageHandleContributor<TextMessage>
{
    public async virtual Task HandleAsync(MessageHandleContext<TextMessage> context)
    {
        var messageSender = context.ServiceProvider.GetRequiredService<IServiceCenterMessageSender>();

        await messageSender.SendAsync(
            new LCH.Abp.WeChat.Official.Services.Models.TextMessageModel(
                context.Message.FromUserName,
                new LCH.Abp.WeChat.Official.Services.Models.TextMessage(
                    context.Message.Content)));
    }
}
