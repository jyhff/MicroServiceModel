using Volo.Abp.Domain.Entities;

namespace LCH.Abp.TextTemplating;
public class TextTemplateDefinitionUpdateDto : TextTemplateDefinitionCreateOrUpdateDto, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; }
}
