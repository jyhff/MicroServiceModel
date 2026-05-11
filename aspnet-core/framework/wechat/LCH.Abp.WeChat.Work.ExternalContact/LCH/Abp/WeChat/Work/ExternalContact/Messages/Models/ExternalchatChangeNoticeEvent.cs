using LCH.Abp.WeChat.Common.Messages;
using LCH.Abp.WeChat.Work.Common.Messages;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Work.ExternalContact.Messages.Models;
/// <summary>
/// 客户群群公告变更事件推送
/// </summary>
[EventName("external_chat_change_notice")]
public class ExternalchatChangeNoticeEvent : ExternalchatUpdateEvent
{
    public override WeChatMessageEto ToEto()
    {
        return new WeChatWorkEventMessageEto<ExternalchatChangeNoticeEvent>(this);
    }
}
