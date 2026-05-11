using Volo.Abp.Domain.Entities;

namespace LCH.Platform.Menus;

public class UserFavoriteMenuUpdateDto : UserFavoriteMenuCreateOrUpdateDto, IHasConcurrencyStamp
{

    public string ConcurrencyStamp { get; set; }
}
