using System.Collections.Generic;
using System.Xml.Serialization;

namespace LCH.Abp.WeChat.Work.ExternalContact.Messages.Models;
/// <summary>
/// 客户群成员变更事件推送
/// </summary>
public abstract class ExternalchatChangeMemberEvent : ExternalchatUpdateEvent
{
    /// <summary>
    /// 成员变更数量
    /// </summary>
    [XmlElement("MemChangeCnt")]
    public int MemChangeCnt { get; set; }
    /// <summary>
    /// 变更的成员列表
    /// </summary>
    [XmlElement("MemChangeList")]
    public List<ExternalchatChangeMember> MemChangeList { get; set; } = new List<ExternalchatChangeMember>();
    /// <summary>
    /// 变更前的群成员版本号
    /// </summary>
    [XmlElement("LastMemVer")]
    public string LastMemVer { get; set; }
    /// <summary>
    /// 变更后的群成员版本号
    /// </summary>
    [XmlElement("CurMemVer")]
    public string CurMemVer { get; set; }
}

public class ExternalchatChangeMember
{
    /// <summary>
    /// 成员Id
    /// </summary>
    [XmlElement("Item")]
    public string UserId { get; set; }
}
