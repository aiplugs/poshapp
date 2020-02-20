using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Aiplugs.PoshApp.ViewModels;
using System.Threading.Tasks;
using ElectronNET.API;

namespace Aiplugs.PoshApp.Controllers
{
    public class DefaultController : Controller
    {
        public async Task<IActionResult> Index()
        {
            if (HybridSupport.IsElectronActive)
            {
                ViewBag.Version = await Electron.App.GetVersionAsync();
            }

            return View();
        }

        [Route("/Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
