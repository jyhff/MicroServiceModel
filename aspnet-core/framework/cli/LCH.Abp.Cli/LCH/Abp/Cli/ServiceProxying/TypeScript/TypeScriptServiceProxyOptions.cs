using System.Collections.Generic;

namespace LCH.Abp.Cli.ServiceProxying.TypeScript;

public class TypeScriptServiceProxyOptions
{
    public IDictionary<string, IHttpApiScriptGenerator> ScriptGenerators { get; }
    public TypeScriptServiceProxyOptions()
    {
        ScriptGenerators = new Dictionary<string, IHttpApiScriptGenerator>();
    }
}
