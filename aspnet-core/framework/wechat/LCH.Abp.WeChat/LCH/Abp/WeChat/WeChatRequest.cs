using System;

namespace LCH.Abp.WeChat;

[Serializable]
public abstract class WeChatRequest
{
    public virtual string SerializeToJson()
    {
        return WeChatObjectSerializeExtensions.SerializeToJson(this);
    }
}
