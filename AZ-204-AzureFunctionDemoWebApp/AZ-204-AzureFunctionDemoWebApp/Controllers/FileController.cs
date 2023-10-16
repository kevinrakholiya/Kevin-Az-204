using Microsoft.AspNetCore.Mvc;

namespace AZ_204_AzureFunctionDemoWebApp.Controllers
{
    public class FileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
