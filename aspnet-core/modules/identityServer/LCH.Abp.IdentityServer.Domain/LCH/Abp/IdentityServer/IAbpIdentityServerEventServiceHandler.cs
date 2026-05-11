using IdentityServer4.Events;
using System.Threading.Tasks;

namespace LCH.Abp.IdentityServer;
public interface IAbpIdentityServerEventServiceHandler
{
    Task RaiseAsync(Event evt);

    bool CanRaiseEventType(EventTypes evtType);
}
