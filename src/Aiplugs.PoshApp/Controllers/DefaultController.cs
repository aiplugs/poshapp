using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Aiplugs.PoshApp.ViewModels;

namespace Aiplugs.PoshApp.Controllers
{
    public class DefaultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
