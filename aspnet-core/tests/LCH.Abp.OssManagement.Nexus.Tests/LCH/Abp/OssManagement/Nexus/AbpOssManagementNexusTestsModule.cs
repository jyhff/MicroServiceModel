using LCH.Abp.BlobStoring.Nexus;
using Volo.Abp.Modularity;

namespace LCH.Abp.OssManagement.Nexus;

[DependsOn(
    typeof(AbpOssManagementNexusModule),
    typeof(AbpBlobStoringNexusTestModule))]
public class AbpOssManagementNexusTestsModule : AbpModule
{

}
