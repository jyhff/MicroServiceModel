using LCH.Abp.WeChat.Common.Messages;
using LCH.Abp.WeChat.Work.Common.Messages;
using System.Xml.Serialization;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Work.Contacts.Messages.Models;
/// <summary>
/// 删除成员事件
/// </summary>
[EventName("delete_user")]
public class DeleteUserEvent : WeChatWorkEventMessage
{
    /// <summary>
    /// 变更信息的成员UserID
    /// </summary>
    [XmlElement("UserID")]
    public string UserId { get; set; }

    public override WeChatMessageEto ToEto()
    {
        return new WeChatWorkEventMessageEto<DeleteUserEvent>(this);
    }
}
