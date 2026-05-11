using Microsoft.AspNetCore.Mvc;

namespace LCH.Abp.Elsa.Designer.Areas.Elsa;

[Route("v{apiVersion:apiVersion}/ElsaAuthentication/logout")]
[Produces("application/json")]
public class ElsaUserLogoutController : Controller
{
    [HttpGet]
    public IActionResult Handle()
    {
        return Redirect("/Account/Logout");
    }
}
