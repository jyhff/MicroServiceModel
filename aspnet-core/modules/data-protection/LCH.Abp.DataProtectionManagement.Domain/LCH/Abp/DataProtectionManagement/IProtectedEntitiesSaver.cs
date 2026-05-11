using System.Threading;
using System.Threading.Tasks;

namespace LCH.Abp.DataProtectionManagement;
public interface IProtectedEntitiesSaver
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
