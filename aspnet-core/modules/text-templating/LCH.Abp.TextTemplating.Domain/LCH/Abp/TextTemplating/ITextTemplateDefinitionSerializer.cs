using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.TextTemplating;

namespace LCH.Abp.TextTemplating;
public interface ITextTemplateDefinitionSerializer
{
    Task<TextTemplateDefinition> SerializeAsync(TemplateDefinition template);
}
