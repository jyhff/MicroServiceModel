using LCH.Abp.WeChat.Common.Messages;
using System.Xml.Serialization;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Official.Messages.Models;
/// <summary>
/// 文本消息
/// </summary>
[EventName("text")]
public class TextMessage : WeChatOfficialGeneralMessage
{
    /// <summary>
    /// 文本消息内容
    /// </summary>
    [XmlElement("Content")]
    public string Content { get; set; }
    public override WeChatMessageEto ToEto()
    {
        return new WeChatOfficialGeneralMessageEto<TextMessage>(this);
    }
}
