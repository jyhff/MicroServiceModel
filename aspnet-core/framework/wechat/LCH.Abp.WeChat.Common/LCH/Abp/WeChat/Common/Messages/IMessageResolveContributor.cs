using System.Threading.Tasks;

namespace LCH.Abp.WeChat.Common.Messages;

public interface IMessageResolveContributor
{
    string Name { get; }

    Task ResolveAsync(IMessageResolveContext context);
}
