using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace LCH.Abp.PermissionManagement.Definitions;
public class PermissionGroupDefinitionUpdateDto : PermissionGroupDefinitionCreateOrUpdateDto, IHasConcurrencyStamp
{
    [StringLength(40)]
    public string ConcurrencyStamp { get; set; }
}
