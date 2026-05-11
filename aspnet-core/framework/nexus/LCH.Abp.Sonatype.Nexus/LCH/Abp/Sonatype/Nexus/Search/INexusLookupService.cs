using LCH.Abp.Sonatype.Nexus.Assets;
using LCH.Abp.Sonatype.Nexus.Components;
using System.Threading;
using System.Threading.Tasks;

namespace LCH.Abp.Sonatype.Nexus.Search;

public interface INexusLookupService
{
    Task<NexusComponentListResult> ListComponentAsync(NexusSearchArgs args, CancellationToken cancellationToken = default);

    Task<NexusAssetListResult> ListAssetAsync(NexusSearchArgs args, CancellationToken cancellationToken = default);
}
