using System.Threading.Tasks;

namespace LCH.Abp.Cli.UI;
public interface IViewGenerator
{
    Task GenerateAsync(GenerateViewArgs args);
}
