using System;
using System.ComponentModel.DataAnnotations;

namespace LCH.Platform.Menus;
public class UserFavoriteMenuRemoveInput
{
    [Required]
    public Guid MenuId { get; set; }
}
