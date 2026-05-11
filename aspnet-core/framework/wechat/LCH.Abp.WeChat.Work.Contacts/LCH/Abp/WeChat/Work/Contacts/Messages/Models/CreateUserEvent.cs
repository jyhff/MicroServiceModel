using LCH.Abp.WeChat.Common.Messages;
using LCH.Abp.WeChat.Work.Common.Messages;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Work.Contacts.Messages.Models;
/// <summary>
/// 新增成员事件
/// </summary>
[EventName("create_user")]
public class CreateUserEvent : UserChangeEvent
{
    public override WeChatMessageEto ToEto()
    {
        return new WeChatWorkEventMessageEto<CreateUserEvent>(this);
    }
}
