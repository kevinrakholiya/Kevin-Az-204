using AZ_204_AzureFunctionDemoWebApp.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AZ_204_AzureFunctionDemoWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string StorageAccountConnectionString;
        private readonly string BlobFileContainer;
        private readonly string StorageAccountName;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            StorageAccountConnectionString = configuration["AppSettings:StorageAccountConnectionString"];
            StorageAccountName = configuration["AppSettings:StorageAccountName"];
            BlobFileContainer = configuration["AppSettings:BlobFileContainer"];
        }

        public async Task<IActionResult> Index()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageAccountConnectionString);

            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobFileContainer);
            if (!blobContainerClient.Exists())
            {
                blobContainerClient = await blobServiceClient.CreateBlobContainerAsync(BlobFileContainer);
            }
            var blobList = new List<BlobViewModel>();
            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
            {
                blobList.Add(new BlobViewModel
                {
                    BlobName = blobItem.Name,
                    LastModified = blobItem.Properties.LastModified,
                    Tier = blobItem.Properties.AccessTier.ToString(),
                    BlobUrl = $"https://{StorageAccountName}.blob.core.windows.net/{BlobFileContainer}/{blobItem.Name}"
                    // Add other properties as needed
                });
            }

            blobList = blobList.OrderByDescending(x => x.LastModified).ToList();
            return View(blobList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}