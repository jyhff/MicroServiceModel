using LCH.Platform.Routes;
using System;
using Volo.Abp.Validation;

namespace LCH.Platform.Menus;

public class UserMenuStartupInput
{
    public Guid UserId { get; set; }


    [DynamicStringLength(typeof(LayoutConsts), nameof(LayoutConsts.MaxFrameworkLength))]
    public string Framework { get; set; }
}
