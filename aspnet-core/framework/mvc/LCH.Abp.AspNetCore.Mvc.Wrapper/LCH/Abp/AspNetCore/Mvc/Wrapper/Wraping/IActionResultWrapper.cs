using Microsoft.AspNetCore.Mvc.Filters;

namespace LCH.Abp.AspNetCore.Mvc.Wrapper.Wraping;

public interface IActionResultWrapper
{
    void Wrap(FilterContext context);
}
