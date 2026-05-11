using LCH.Abp.WeChat.Common.Messages;
using LCH.Abp.WeChat.Work.Common.Messages;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Work.ExternalContact.Messages.Models;
/// <summary>
/// 客户群解散事件推送
/// </summary>
[EventName("external_chat_dismiss")]
public class ExternalchatDismissEvent : ExternalchatChangeEvent
{
    public override WeChatMessageEto ToEto()
    {
        return new WeChatWorkEventMessageEto<ExternalchatDismissEvent>(this);
    }
}
