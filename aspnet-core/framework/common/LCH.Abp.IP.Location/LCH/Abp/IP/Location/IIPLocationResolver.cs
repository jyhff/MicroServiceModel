using JetBrains.Annotations;
using System.Threading.Tasks;

namespace LCH.Abp.IP.Location;
public interface IIPLocationResolver
{
    [NotNull]
    Task<IPLocationResolveResult> ResolveAsync(string ipAddress);
}
