using System.Threading.Tasks;

namespace LCH.Abp.IM.Messages;

public interface IMessageSenderProvider
{
    string Name { get; }
    Task SendMessageAsync(ChatMessage chatMessage);
}
