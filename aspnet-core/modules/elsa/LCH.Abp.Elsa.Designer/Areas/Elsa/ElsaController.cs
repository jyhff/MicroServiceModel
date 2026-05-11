using Microsoft.AspNetCore.Mvc;

namespace LCH.Abp.Elsa.Designer.Areas.Elsa;

public class ElsaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
