using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LCH.Abp.Applications;

[DependsOn(
    typeof(AbpAutofacModule))]
public class AbpApplicationsModule : AbpModule
{
}
