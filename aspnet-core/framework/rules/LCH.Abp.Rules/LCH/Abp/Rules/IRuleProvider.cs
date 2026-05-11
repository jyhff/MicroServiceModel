using System.Threading;
using System.Threading.Tasks;

namespace LCH.Abp.Rules;

public interface IRuleProvider
{
    Task ExecuteAsync<T>(T input, object[] @params = null, CancellationToken cancellationToken = default);
}
