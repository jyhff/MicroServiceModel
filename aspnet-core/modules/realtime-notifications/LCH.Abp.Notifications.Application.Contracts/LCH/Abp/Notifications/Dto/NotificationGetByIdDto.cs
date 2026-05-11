using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.Notifications;

public class NotificationGetByIdDto
{
    [Required]
    [DisplayName("Notifications:Id")]
    public long NotificationId { get; set; }
}
