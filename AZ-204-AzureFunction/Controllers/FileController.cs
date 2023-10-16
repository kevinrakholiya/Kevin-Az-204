using AZ_204_AzureFunction.Models;
using Microsoft.AspNetCore.Mvc;

namespace AZ_204_AzureFunction.Controllers
{
    public class FileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        
    }
}
