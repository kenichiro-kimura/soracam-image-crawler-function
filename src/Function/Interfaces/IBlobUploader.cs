namespace soracamChecker.Interfaces
{
    public interface IBlobUploader
    {
        Task<BlobClient> GetBlobClientAsync(string containerName,string blobName);
        Task<BlobContentInfo> UploadAsync(BlobClient blobClient, Stream stream);
    }
}
