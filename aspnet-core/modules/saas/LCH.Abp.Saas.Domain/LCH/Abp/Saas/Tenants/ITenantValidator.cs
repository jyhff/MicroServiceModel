using System.Threading.Tasks;

namespace LCH.Abp.Saas.Tenants;
public interface ITenantValidator
{
    Task ValidateAsync(Tenant tenant);
}
