using LCH.Abp.WeChat.Common.Messages;
using Volo.Abp.EventBus;

namespace LCH.Abp.WeChat.Official.Messages;

[GenericEventName(Prefix = "wechat.official.messages.")]
public class WeChatOfficialGeneralMessageEto<TMessage> : WeChatMessageEto
    where TMessage : WeChatOfficialGeneralMessage
{
    public TMessage Message { get; set; }
    public WeChatOfficialGeneralMessageEto()
    {

    }
    public WeChatOfficialGeneralMessageEto(TMessage message)
    {
        Message = message;
    }
}
