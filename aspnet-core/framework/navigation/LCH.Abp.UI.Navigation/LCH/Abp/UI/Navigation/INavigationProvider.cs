using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCH.Abp.UI.Navigation;

public interface INavigationProvider
{
    Task<IReadOnlyCollection<ApplicationMenu>> GetAllAsync();
}
