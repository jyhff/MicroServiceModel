using LCH.Platform.Routes;
using Volo.Abp.Validation;

namespace LCH.Platform.Menus;
public class UserFavoriteMenuGetListInput
{
    [DynamicStringLength(typeof(LayoutConsts), nameof(LayoutConsts.MaxFrameworkLength))]
    public string Framework { get; set; }
}
