using LCH.Abp.WeChat.Common.Messages;
using LCH.Abp.WeChat.Work.Common.Messages;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Work.ExternalContact.Messages.Models;
/// <summary>
/// 编辑企业客户事件推送
/// </summary>
[EventName("external_contact_update")]
public class ExternalContactUpdateEvent : ExternalContactChangeEvent
{
    public override WeChatMessageEto ToEto()
    {
        return new WeChatWorkEventMessageEto<ExternalContactUpdateEvent>(this);
    }
}

