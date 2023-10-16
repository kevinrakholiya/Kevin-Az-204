namespace AZ_204_AzureFunctionDemoWebApp.Models
{
    public class BlobViewModel
    {
        public string BlobName { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public string Tier { get; set; }
        public string BlobUrl { get; set; }
    }
}
