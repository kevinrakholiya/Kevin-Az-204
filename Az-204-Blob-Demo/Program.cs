using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

public class Program
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();
    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Azure Blob Storage exercise\n");
            ProcessAsync().GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
        }
        finally
        {
            Console.WriteLine("End of program, press any key to exit.");
            Console.ReadKey();
        }
    }

    private static async Task ProcessAsync()
    {
        // Copy the connection string from the portal in the variable below.
        string storageConnectionString = Configuration["AppSettings:StorageConnectionString"] ?? "";

        // Create a client that can authenticate with a connection string
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

        //Create a unique name for the container
        string containerName = Configuration["AppSettings:ContainerName"] ?? "";

        // Create Container Client If Not Exists
        BlobContainerClient blobContainerClient = await CreateContainerIfNotExists(blobServiceClient, containerName);

        //Upload Blob to Container
        await UploadBlobToContainer(blobContainerClient);

        //List Blob in Container
        await ListOfBlobsInContainer(blobContainerClient);

        //Read Container Property
        await ReadContainerPropertiesAsync(blobContainerClient);

        //Set Container Metadata
        await AddContainerMetadataAsync(blobContainerClient);

        //Read Container Metadata
        await ReadContainerMetadataAsync(blobContainerClient);
    }

    private static async Task<BlobContainerClient> CreateContainerIfNotExists(BlobServiceClient blobServiceClient, string containerName)
    {
        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        if (!blobContainerClient.Exists())
        {
            blobContainerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
            Console.WriteLine("Create New Blob Container Successfully.");
        }
        else
        {
            Console.WriteLine("Container Found Successfully");
        }

        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();

        return blobContainerClient;
    }

    private static async Task UploadBlobToContainer(BlobContainerClient blobContainerClient)
    {
        // Create a local file in the ./data/ directory for uploading and downloading
        string blobFileLocalPath = Configuration["AppSettings:BlobFileLocalPath"] ?? "";
        string fileName = "blobfile" + Guid.NewGuid().ToString() + ".txt";
        string localFilePath = Path.Combine(blobFileLocalPath, fileName);
        // Check if the directory exists; if not, create it
        if (!Directory.Exists(blobFileLocalPath))
        {
            Directory.CreateDirectory(blobFileLocalPath);
        }
        // Check if the file exists; if not, create it
        Console.WriteLine("Enter text you want to write in blob file : ");
        var fileText = Console.ReadLine();
        if (!File.Exists(localFilePath))
        {
            await File.WriteAllTextAsync(localFilePath, fileText);
        }

        BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
        using (FileStream uploadFileStream = File.OpenRead(localFilePath))
        {
            await blobClient.UploadAsync(uploadFileStream);
            uploadFileStream.Close();
        }

        Console.WriteLine("\nThe file was uploaded. We'll Download This file Now.");
        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();
        //Download Blob
        await DownLoadBlob(localFilePath, blobClient);

    }

    private static async Task ListOfBlobsInContainer(BlobContainerClient blobContainerClient)
    {
        // List blobs in the container
        Console.WriteLine("Listing blobs...");

        await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
        {
            Console.WriteLine("\t" + blobItem.Name);
        }

        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();
    }

    private static async Task DownLoadBlob(string localFilePath, BlobClient blobClient)
    {
        string downloadFilePath = localFilePath.Replace(".txt", "DOWNLOADED.txt");

        Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

        BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();

        using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
        {
            await blobDownloadInfo.Content.CopyToAsync(downloadFileStream);
        }

        Console.WriteLine("The next step is to list the container blobs.");
        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();
    }

    private static async Task ReadContainerPropertiesAsync(BlobContainerClient container)
    {
        try
        {
            // Fetch some container properties and write out their values.
            var properties = await container.GetPropertiesAsync();
            Console.WriteLine($"Properties for container {container.Uri}");
            Console.WriteLine($"Public access level: {properties.Value.PublicAccess}");
            Console.WriteLine($"Last modified time in UTC: {properties.Value.LastModified}");
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
            Console.WriteLine(e.Message);
            Console.ReadLine();
        }
    }

    private static async Task AddContainerMetadataAsync(BlobContainerClient container)
    {
        try
        {
            IDictionary<string, string> metadata = new Dictionary<string, string>
            {
                // Add some metadata to the container.
                { "docType", "textDocuments" },
                { "category", "guidance" },
                { "category1", "guidance" }
            };

            // Set the container's metadata.
            await container.SetMetadataAsync(metadata);
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
            Console.WriteLine(e.Message);
            Console.ReadLine();
        }
    }

    private static async Task ReadContainerMetadataAsync(BlobContainerClient container)
    {
        try
        {
            var properties = await container.GetPropertiesAsync();

            // Enumerate the container's metadata.
            Console.WriteLine("Container metadata:");
            foreach (var metadataItem in properties.Value.Metadata)
            {
                Console.WriteLine($"\tKey: {metadataItem.Key}");
                Console.WriteLine($"\tValue: {metadataItem.Value}");
            }
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
            Console.WriteLine(e.Message);
            Console.ReadLine();
        }
    }
}