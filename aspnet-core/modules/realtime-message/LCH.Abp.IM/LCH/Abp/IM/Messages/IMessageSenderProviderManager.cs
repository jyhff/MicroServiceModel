using System.Collections.Generic;

namespace LCH.Abp.IM.Messages;

public interface IMessageSenderProviderManager
{
    List<IMessageSenderProvider> Providers { get; }
}
