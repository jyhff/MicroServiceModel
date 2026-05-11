using LCH.Abp.WeChat.Common.Messages.Handlers;
using LCH.Abp.WeChat.Official.Messages.Models;
using LCH.Abp.WeChat.Official.Services;

namespace PackageName.CompanyName.ProjectName.AIO.Host.WeChat.Official.Messages;
/// <summary>
/// 用户关注回复消息
/// </summary>
public class UserSubscribeEventContributor : IEventHandleContributor<UserSubscribeEvent>
{
    public async virtual Task HandleAsync(MessageHandleContext<UserSubscribeEvent> context)
    {
        var messageSender = context.ServiceProvider.GetRequiredService<IServiceCenterMessageSender>();

        await messageSender.SendAsync(
            new LCH.Abp.WeChat.Official.Services.Models.TextMessageModel(
                context.Message.FromUserName,
                new LCH.Abp.WeChat.Official.Services.Models.TextMessage(
                    "感谢您的关注, 点击菜单了解更多.")));
    }
}
