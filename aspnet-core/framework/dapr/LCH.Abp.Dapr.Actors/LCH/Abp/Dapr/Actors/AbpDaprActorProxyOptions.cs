using LCH.Abp.Dapr.Actors.DynamicProxying;
using System;
using System.Collections.Generic;

namespace LCH.Abp.Dapr.Actors;

public class AbpDaprActorProxyOptions
{
    public Dictionary<Type, DynamicDaprActorProxyConfig> ActorProxies { get; set; }

    public AbpDaprActorProxyOptions()
    {
        ActorProxies = new Dictionary<Type, DynamicDaprActorProxyConfig>();
    }
}
