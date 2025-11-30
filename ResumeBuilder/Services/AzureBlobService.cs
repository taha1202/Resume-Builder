using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ResumeBuilder.Services
{
    public class AzureBlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"] ?? "";
            _containerName = configuration["AzureStorage:ContainerName"] ?? "resumes";
        }

        public async Task<string> UploadResumeAsync(string fileName, Stream content)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.UploadAsync(content, true);

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading to blob: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<bool> DeleteResumeAsync(string fileName)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                return await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}