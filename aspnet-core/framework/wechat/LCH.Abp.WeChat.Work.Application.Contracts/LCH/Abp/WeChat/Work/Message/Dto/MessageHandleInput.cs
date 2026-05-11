using LCH.Abp.WeChat.Work.Models;
using System;
using Volo.Abp.Auditing;

namespace LCH.Abp.WeChat.Work.Message;

[Serializable]
public class MessageHandleInput : WeChatWorkMessage
{
    [DisableAuditing]
    public string Data { get; set; }
}
