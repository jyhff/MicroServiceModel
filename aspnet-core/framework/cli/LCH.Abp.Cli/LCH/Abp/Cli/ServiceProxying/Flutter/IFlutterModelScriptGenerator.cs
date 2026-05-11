using Volo.Abp.Http.Modeling;

namespace LCH.Abp.Cli.ServiceProxying.Flutter;

public interface IFlutterModelScriptGenerator
{
    string CreateScript(
        ApplicationApiDescriptionModel appModel,
        ControllerApiDescriptionModel actionModel);
}
