using System.Collections.Generic;

namespace LCH.Abp.UI.Navigation;

public interface INavigationDefinitionManager
{
    IReadOnlyList<NavigationDefinition> GetAll();
}
