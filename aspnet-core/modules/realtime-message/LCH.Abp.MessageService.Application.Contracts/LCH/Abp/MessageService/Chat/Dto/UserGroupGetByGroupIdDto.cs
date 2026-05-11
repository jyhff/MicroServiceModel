using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.MessageService.Chat;

public class UserGroupGetByGroupIdDto
{
    [Required]
    public long GroupId { get; set; }
}
