using System.Threading.Tasks;

namespace LCH.Abp.Saas.Editions;

public interface IEditionDataSeeder
{
    Task SeedDefaultEditionsAsync();
}
