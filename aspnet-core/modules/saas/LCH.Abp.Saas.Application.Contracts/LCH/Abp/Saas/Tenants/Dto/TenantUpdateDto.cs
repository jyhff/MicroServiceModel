using Volo.Abp.Domain.Entities;

namespace LCH.Abp.Saas.Tenants;
public class TenantUpdateDto : TenantCreateOrUpdateBase, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; }
}