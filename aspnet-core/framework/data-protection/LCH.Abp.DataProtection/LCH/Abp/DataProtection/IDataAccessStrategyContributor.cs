using System.Threading.Tasks;

namespace LCH.Abp.DataProtection;

public interface IDataAccessStrategyContributor
{
    string Name { get; }
    Task<DataAccessStrategyState> GetOrNullAsync(DataAccessStrategyContributorContext context);
}
