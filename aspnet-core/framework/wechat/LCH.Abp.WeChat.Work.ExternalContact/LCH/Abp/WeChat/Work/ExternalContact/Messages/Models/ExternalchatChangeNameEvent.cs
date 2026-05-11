using LCH.Abp.WeChat.Common.Messages;
using LCH.Abp.WeChat.Work.Common.Messages;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Work.ExternalContact.Messages.Models;
/// <summary>
/// 客户群群名变更事件推送
/// </summary>
[EventName("external_chat_change_name")]
public class ExternalchatChangeNameEvent : ExternalchatUpdateEvent
{
    public override WeChatMessageEto ToEto()
    {
        return new WeChatWorkEventMessageEto<ExternalchatChangeNameEvent>(this);
    }
}
