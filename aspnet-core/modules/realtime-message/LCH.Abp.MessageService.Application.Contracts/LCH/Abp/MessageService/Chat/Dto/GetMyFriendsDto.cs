using Volo.Abp.Application.Dtos;

namespace LCH.Abp.MessageService.Chat;

public class GetMyFriendsDto : ISortedResultRequest
{
    public string Sorting { get; set; }
}
