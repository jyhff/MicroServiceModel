using System.Threading.Tasks;

namespace LCH.Platform.Messages;
public interface ISmsMessageManager
{
    Task<SmsMessage> SendAsync(SmsMessage message);
}
