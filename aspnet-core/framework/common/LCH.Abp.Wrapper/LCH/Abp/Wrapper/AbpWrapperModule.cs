using Volo.Abp.ExceptionHandling;
using Volo.Abp.Modularity;

namespace LCH.Abp.Wrapper;

[DependsOn(typeof(AbpExceptionHandlingModule))]
public class AbpWrapperModule: AbpModule
{

}
