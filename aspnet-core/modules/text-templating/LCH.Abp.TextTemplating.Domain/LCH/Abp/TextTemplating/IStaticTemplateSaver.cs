using System.Threading.Tasks;

namespace LCH.Abp.TextTemplating;
public interface IStaticTemplateSaver
{
    Task SaveDefinitionTemplateAsync();

    Task SaveTemplateContentAsync();
}
