using System.Diagnostics;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Projects");
        return RedirectToAction("Login", "Account");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
