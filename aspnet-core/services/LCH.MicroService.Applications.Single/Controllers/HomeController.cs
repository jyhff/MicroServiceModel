using Microsoft.AspNetCore.Mvc;

namespace LCH.MicroService.Applications.Single.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
