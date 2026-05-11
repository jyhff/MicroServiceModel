using Volo.Abp.Collections;

namespace LCH.Abp.IdentityServer;
public class AbpIdentityServerEventOptions
{
    public ITypeList<IAbpIdentityServerEventServiceHandler> EventServiceHandlers { get; }
    public AbpIdentityServerEventOptions()
    {
        EventServiceHandlers = new TypeList<IAbpIdentityServerEventServiceHandler>();
    }
}
