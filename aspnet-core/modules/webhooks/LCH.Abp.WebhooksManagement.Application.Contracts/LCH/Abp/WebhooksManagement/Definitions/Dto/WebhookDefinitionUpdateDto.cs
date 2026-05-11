using LCH.Abp.WebhooksManagement.Definitions.Dto;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace LCH.Abp.WebhooksManagement.Definitions;
public class WebhookDefinitionUpdateDto : WebhookDefinitionCreateOrUpdateDto, IHasConcurrencyStamp
{
    [StringLength(40)]
    public string ConcurrencyStamp { get; set; }
}
