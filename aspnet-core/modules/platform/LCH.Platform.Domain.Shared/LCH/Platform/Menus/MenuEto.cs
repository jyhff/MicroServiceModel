using LCH.Platform.Routes;
using Volo.Abp.EventBus;

namespace LCH.Platform.Menus;

[EventName("platform.menus.menu")]
public class MenuEto : RouteEto
{
    public string Framework { get; set; }
}
