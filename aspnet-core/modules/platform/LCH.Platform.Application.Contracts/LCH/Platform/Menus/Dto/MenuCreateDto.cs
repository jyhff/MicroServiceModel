using System;
using System.ComponentModel.DataAnnotations;

namespace LCH.Platform.Menus;

public class MenuCreateDto : MenuCreateOrUpdateDto
{
    [Required]
    public Guid LayoutId { get; set; }
}
