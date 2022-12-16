namespace soracamChecker.Services
{
    public class BlobUploader : IBlobUploader
    {
        private readonly BlobServiceClient blobServiceClient;

        public BlobUploader(IConfiguration configuration)
        {
            var connectionString = configuration?.GetConnectionString("StorageConnectionString") ?? throw new ArgumentNullException(nameof(configuration));
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                this.blobServiceClient = new BlobServiceClient(connectionString);
            }
        }

        public async Task<BlobClient> GetBlobClientAsync(string blobName, string containerName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            return containerClient.GetBlobClient(blobName);
        }

        public async Task<BlobContentInfo> UploadAsync(BlobClient blobClient, Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.Flush();
            stream.Position = 0;
            return await blobClient.UploadAsync(stream, true);
        }
    }
}
