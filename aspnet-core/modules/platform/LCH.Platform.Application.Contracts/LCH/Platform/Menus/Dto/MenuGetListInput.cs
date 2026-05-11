using LCH.Platform.Routes;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace LCH.Platform.Menus;

public class MenuGetListInput : PagedAndSortedResultRequestDto
{
    [DynamicStringLength(typeof(LayoutConsts), nameof(LayoutConsts.MaxFrameworkLength))]
    public string Framework { get; set; }

    public string Filter { get; set; }

    public Guid? ParentId { get; set; }

    public Guid? LayoutId { get; set; }
}
