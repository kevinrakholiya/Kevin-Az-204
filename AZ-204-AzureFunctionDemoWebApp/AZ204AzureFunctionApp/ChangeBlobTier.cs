using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System.Threading.Tasks;

namespace AZ204AzureFunctionApp
{
    public class ChangeBlobTier
    {
        static readonly string StorageAccountConnectionString = Environment.GetEnvironmentVariable("StorageAccountConnectionString");
        static readonly string BlobFileContainer = Environment.GetEnvironmentVariable("BlobFileContainer");
        static readonly int ChangeToCoolTimeOffset = Convert.ToInt32(Environment.GetEnvironmentVariable("ChangeToCoolTimeOffset"));
        [FunctionName("ChangeBlobTier")]
        public static async Task RunAsync([TimerTrigger("0 0 0 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageAccountConnectionString);

            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobFileContainer);
            if (!blobContainerClient.Exists())
            {
                blobContainerClient = await blobServiceClient.CreateBlobContainerAsync(BlobFileContainer);
            }

            DateTime changeToCoolTimeOffset = DateTime.UtcNow.AddMinutes(ChangeToCoolTimeOffset);

            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
            {
                if (blobItem.Properties.LastModified < changeToCoolTimeOffset && blobItem.Properties.AccessTier.ToString().Contains("Hot"))
                {
                    BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                    await blobClient.SetAccessTierAsync(AccessTier.Cool);
                    log.LogInformation($"Changed blob '{blobClient.Name}' to Cool tier.");
                }
            }
        }
    }
}
