using System.Threading.Tasks;

namespace LCH.Abp.Idempotent;

public interface IIdempotentChecker
{
    Task<IdempotentGrantResult> IsGrantAsync(IdempotentCheckContext context);
}
