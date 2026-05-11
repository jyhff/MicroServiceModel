using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LCH.Abp.WeChat.Work.ExternalContact.GroupChats.Models;
/// <summary>
/// 邀请者
/// </summary>
public class GroupChatInvitor
{
    /// <summary>
    /// 邀请者的userid
    /// </summary>
    [NotNull]
    [JsonProperty("userid")]
    [JsonPropertyName("userid")]
    public string UserId { get; set; }
}
