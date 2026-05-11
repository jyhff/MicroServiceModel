using System.Threading.Tasks;

namespace LCH.MicroService.IdentityServer.EntityFrameworkCore.DataSeeder;

public interface IWeChatResourceDataSeeder
{
    Task CreateStandardResourcesAsync();
}
