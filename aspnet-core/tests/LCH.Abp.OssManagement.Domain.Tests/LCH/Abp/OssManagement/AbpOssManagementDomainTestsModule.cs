using LCH.Abp.Tests;
using Volo.Abp.Modularity;

namespace LCH.Abp.OssManagement;

[DependsOn(
    typeof(AbpOssManagementDomainModule),
    typeof(AbpTestsBaseModule))]
public class AbpOssManagementDomainTestsModule : AbpModule
{
}
