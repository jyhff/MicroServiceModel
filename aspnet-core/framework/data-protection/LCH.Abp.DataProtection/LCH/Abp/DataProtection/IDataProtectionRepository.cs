using System.Linq;
using System.Threading.Tasks;

namespace LCH.Abp.DataProtection;
public interface IDataProtectionRepository<TEntity> : IDataProtectedEnabled
{
    Task<IQueryable<TEntity>> GetQueryableAsync();
}
