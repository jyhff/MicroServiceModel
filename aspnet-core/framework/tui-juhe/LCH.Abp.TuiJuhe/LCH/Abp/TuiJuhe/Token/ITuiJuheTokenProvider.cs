using System.Threading;
using System.Threading.Tasks;

namespace LCH.Abp.TuiJuhe.Token;

public interface ITuiJuheTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}
