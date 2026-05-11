using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LCH.Abp.DataProtection
{
    public interface IFakeProtectionObjectRepository : IRepository<FakeProtectionObject, int>
    {
        Task<List<FakeProtectionObject>> GetAllListAsync();
    }
}
