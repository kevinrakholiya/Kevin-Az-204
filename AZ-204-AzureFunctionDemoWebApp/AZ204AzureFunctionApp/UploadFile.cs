using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;

namespace AZ204AzureFunctionApp
{
    public static class UploadFile
    {
        static readonly string StorageAccountConnectionString = Environment.GetEnvironmentVariable("StorageAccountConnectionString");
        static readonly string BlobFileContainer = Environment.GetEnvironmentVariable("BlobFileContainer");
        [FunctionName("UploadFile")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                string error;
                string originalFileName = null;
                string fileName = null;
                IFormFile file = null;
                IFormFile Image = req.Form.Files["File"];
                if (req.Form.Files.Count != 0)
                {
                    originalFileName = Image.FileName;
                    fileName = Path.GetFileNameWithoutExtension(originalFileName) + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") + DateTime.Now.ToString("yyyy") +
                       DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm") + DateTime.Now.ToString("ss") +
                       System.IO.Path.GetExtension(originalFileName);
                    file = req.Form.Files[0];
                }

                if (file != null)
                {
                    error = await UploadImage(file, fileName, StorageAccountConnectionString);
                    if(string.IsNullOrEmpty(error))
                    {
                        return new OkObjectResult("File Uploaded Successfully");
                    }
                    else
                    {
                        return new BadRequestObjectResult(error);
                    }
                }
                else
                {
                    return new OkObjectResult("File Not Found");
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public static async Task<string> UploadImage(IFormFile file, string fileName, string StorageAccountConnectionString)
        {
            string error = string.Empty;
            try
            {
                if (!String.IsNullOrEmpty(fileName))
                {
                    BlobContainerClient blobContainerClient = new BlobContainerClient(StorageAccountConnectionString, BlobFileContainer);
                    blobContainerClient.CreateIfNotExists();

                    Stream myBlob = new MemoryStream();
                    myBlob = file.OpenReadStream();
                    var blob = blobContainerClient.GetBlobClient(fileName);
                    await blob.UploadAsync(myBlob);
                }
            }
            catch(Exception ex)
            {
                error = ex.Message;
            }
            return error;
        }
    }
}
