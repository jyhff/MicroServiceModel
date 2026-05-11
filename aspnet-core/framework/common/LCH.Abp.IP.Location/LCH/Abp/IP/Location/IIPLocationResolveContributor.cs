using System.Threading.Tasks;

namespace LCH.Abp.IP.Location;
public interface IIPLocationResolveContributor
{
    string Name { get; }

    Task ResolveAsync(IIPLocationResolveContext context);
}
