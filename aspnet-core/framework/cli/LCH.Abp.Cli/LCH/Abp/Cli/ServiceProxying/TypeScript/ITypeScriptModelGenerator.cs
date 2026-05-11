using Volo.Abp.Http.Modeling;

namespace LCH.Abp.Cli.ServiceProxying.TypeScript;

public interface ITypeScriptModelGenerator
{
    string CreateScript(
        ApplicationApiDescriptionModel appModel,
        ControllerApiDescriptionModel actionModel);
}
