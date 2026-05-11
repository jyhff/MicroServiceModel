using System.ComponentModel.DataAnnotations;
using Volo.Abp.Validation;

namespace LCH.Abp.Notifications.Definitions.Groups;

public class NotificationGroupDefinitionCreateDto : NotificationGroupDefinitionCreateOrUpdateDto
{
    [Required]
    [DynamicStringLength(typeof(NotificationDefinitionGroupRecordConsts), nameof(NotificationDefinitionGroupRecordConsts.MaxNameLength))]
    public string Name { get; set; }
}
