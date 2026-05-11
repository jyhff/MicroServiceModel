using LCH.Abp.Account.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCH.Abp.Account.Web.ExternalProviders;

public interface IExternalProviderService
{
    Task<List<ExternalLoginProviderModel>> GetAllAsync();
}
