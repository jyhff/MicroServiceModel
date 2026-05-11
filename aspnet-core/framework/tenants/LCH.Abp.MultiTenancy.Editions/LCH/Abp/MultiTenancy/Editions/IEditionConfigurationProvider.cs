using System;
using System.Threading.Tasks;

namespace LCH.Abp.MultiTenancy.Editions;

public interface IEditionConfigurationProvider
{
    Task<EditionConfiguration> GetAsync(Guid? tenantId = null);
}
