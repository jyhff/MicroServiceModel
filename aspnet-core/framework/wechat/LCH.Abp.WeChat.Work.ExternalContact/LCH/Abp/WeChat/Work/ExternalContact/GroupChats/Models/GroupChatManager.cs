using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LCH.Abp.WeChat.Work.ExternalContact.GroupChats.Models;
/// <summary>
/// 群管理员
/// </summary>
public class GroupChatManager
{
    /// <summary>
    /// 群管理员userid
    /// </summary>
    [NotNull]
    [JsonProperty("userid")]
    [JsonPropertyName("userid")]
    public string UserId { get; set; }
}
