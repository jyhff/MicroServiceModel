using System.Threading.Tasks;

namespace LCH.Abp.DataProtection;

public interface IDataAccessStrategyStateProvider
{
    Task<DataAccessStrategyState> GetOrNullAsync();
}
