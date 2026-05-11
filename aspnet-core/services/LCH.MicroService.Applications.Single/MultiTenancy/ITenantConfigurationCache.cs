using Volo.Abp.MultiTenancy;

namespace LCH.MicroService.Applications.Single.MultiTenancy;

public interface ITenantConfigurationCache
{
    Task RefreshAsync();

    Task<List<TenantConfiguration>> GetTenantsAsync();
}
