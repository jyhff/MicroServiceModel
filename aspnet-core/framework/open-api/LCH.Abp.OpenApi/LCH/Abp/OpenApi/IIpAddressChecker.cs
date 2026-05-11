using System.Threading;
using System.Threading.Tasks;

namespace LCH.Abp.OpenApi;
public interface IIpAddressChecker
{
    Task<bool> IsGrantAsync(string ipAddress, CancellationToken cancellationToken = default);
}
