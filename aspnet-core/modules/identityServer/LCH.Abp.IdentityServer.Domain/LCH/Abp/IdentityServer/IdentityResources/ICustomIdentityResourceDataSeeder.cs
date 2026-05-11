using System.Threading.Tasks;

namespace LCH.Abp.IdentityServer.IdentityResources;

public interface ICustomIdentityResourceDataSeeder
{
    Task CreateCustomResourcesAsync();
}
