using System.Threading.Tasks;

namespace LCH.Abp.IM.Messages;

public interface IMessageSender
{
    Task<string> SendMessageAsync(ChatMessage chatMessage);
}
