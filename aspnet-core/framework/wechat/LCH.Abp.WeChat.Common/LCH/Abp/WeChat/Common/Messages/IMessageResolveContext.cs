using System.Xml.Linq;
using Volo.Abp.DependencyInjection;

namespace LCH.Abp.WeChat.Common.Messages;
public interface IMessageResolveContext : IServiceProviderAccessor
{
    string Origin { get; }
    XDocument MessageData { get; }
    bool Handled { get; set; }
    WeChatMessage Message { get; set; }
}
