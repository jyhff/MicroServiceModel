using Volo.Abp.EventBus;

namespace LCH.Platform.Messages;

[EventName("platform.messages.sms")]
public class SmsMessageEto : MessageEto
{
}
