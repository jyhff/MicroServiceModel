using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;

namespace LCH.MicroService.PlatformManagement.MultiTenancy;

public interface ITenantConfigurationCache
{
    Task<List<TenantConfiguration>> GetTenantsAsync();
}
