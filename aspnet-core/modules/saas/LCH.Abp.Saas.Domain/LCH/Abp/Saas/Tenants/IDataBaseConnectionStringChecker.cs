using System.Threading.Tasks;

namespace LCH.Abp.Saas.Tenants;

public interface IDataBaseConnectionStringChecker
{
    Task<DataBaseConnectionStringCheckResult> CheckAsync(string connectionString);
}
