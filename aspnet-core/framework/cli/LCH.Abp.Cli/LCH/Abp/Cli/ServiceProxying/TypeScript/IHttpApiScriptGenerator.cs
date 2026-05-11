using Volo.Abp.Http.Modeling;

namespace LCH.Abp.Cli.ServiceProxying.TypeScript;

public interface IHttpApiScriptGenerator
{
    string CreateScript(
        ApplicationApiDescriptionModel appModel,
        ModuleApiDescriptionModel apiModel,
        ControllerApiDescriptionModel actionModel);
}
