using LCH.Abp.WeChat.Common.Messages;
using LCH.Abp.WeChat.Work.Common.Messages;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Work.ExternalContact.Messages.Models;
/// <summary>
/// 删除跟进成员事件
/// </summary>
[EventName("external_contact_del_follow_user")]
public class ExternalContactDeleteFollowUserEvent : ExternalContactChangeEvent
{
    public override WeChatMessageEto ToEto()
    {
        return new WeChatWorkEventMessageEto<ExternalContactDeleteFollowUserEvent>(this);
    }
}
