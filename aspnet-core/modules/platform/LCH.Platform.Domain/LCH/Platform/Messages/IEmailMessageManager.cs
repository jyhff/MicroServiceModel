using System.Threading.Tasks;

namespace LCH.Platform.Messages;
public interface IEmailMessageManager
{
    Task<EmailMessage> SendAsync(EmailMessage message);
}
