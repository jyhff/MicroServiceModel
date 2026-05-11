using JetBrains.Annotations;
using LCH.Abp.WeChat.Work.Contacts.Members.Models;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LCH.Abp.WeChat.Work.Contacts.Members.Response;
/// <summary>
/// 获取部门成员响应参数
/// </summary>
/// <remarks>
/// 详情见: <see href="https://developer.work.weixin.qq.com/document/path/90200" />
/// </remarks>
public class WeChatWorkGetSimpleMemberListResponse : WeChatWorkResponse
{
    /// <summary>
    /// 成员列表
    /// </summary>
    [NotNull]
    [JsonProperty("userlist")]
    [JsonPropertyName("userlist")]
    public DepartmentMember[] UserList { get; set; }
}
