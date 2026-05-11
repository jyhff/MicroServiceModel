using System;
using System.Threading.Tasks;

namespace LCH.Abp.MultiTenancy.Editions;

public interface IEditionStore
{
    Task<EditionInfo> FindByTenantAsync(Guid tenantId);
}
