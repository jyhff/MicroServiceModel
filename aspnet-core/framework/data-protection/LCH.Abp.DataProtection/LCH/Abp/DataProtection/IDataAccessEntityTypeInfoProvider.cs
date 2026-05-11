using LCH.Abp.DataProtection.Models;
using System.Threading.Tasks;

namespace LCH.Abp.DataProtection;

public interface IDataAccessEntityTypeInfoProvider
{
    Task<EntityTypeInfoModel> GetEntitTypeInfoAsync(DataAccessEntitTypeInfoContext context);
}
