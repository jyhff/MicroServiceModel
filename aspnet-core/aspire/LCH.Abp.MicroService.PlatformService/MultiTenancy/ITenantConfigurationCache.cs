using Volo.Abp.MultiTenancy;

namespace LCH.Abp.MicroService.PlatformService.MultiTenancy;

public interface ITenantConfigurationCache
{
    Task<List<TenantConfiguration>> GetTenantsAsync();
}
