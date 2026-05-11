using LCH.Abp.WeChat.Common.Messages;
using LCH.Abp.WeChat.Work.Common.Messages;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Work.ExternalContact.Messages.Models;
/// <summary>
/// 客户群群主变更事件推送
/// </summary>
[EventName("external_chat_change_owner")]
public class ExternalchatChangeOwnerEvent : ExternalchatUpdateEvent
{
    public override WeChatMessageEto ToEto()
    {
        return new WeChatWorkEventMessageEto<ExternalchatChangeOwnerEvent>(this);
    }
}
