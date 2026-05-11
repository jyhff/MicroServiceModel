using LCH.Abp.Sonatype.Nexus;
using Volo.Abp.BlobStoring;
using Volo.Abp.Modularity;

namespace LCH.Abp.BlobStoring.Nexus;

[DependsOn(
    typeof(AbpBlobStoringModule),
    typeof(AbpSonatypeNexusModule))]
public class AbpBlobStoringNexusModule : AbpModule
{
}
