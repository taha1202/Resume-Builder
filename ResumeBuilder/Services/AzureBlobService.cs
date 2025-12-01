using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace ResumeBuilder.Services
{
    public class AzureBlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly string _profileImageContainer;

        public AzureBlobService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"] ?? "";
            _containerName = configuration["AzureStorage:ContainerName"] ?? "resumes";
            _profileImageContainer = configuration["AzureStorage:ProfileImageContainer"] ?? "user-images";
        }

        public async Task<string> UploadResumeAsync(string fileName, Stream content)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

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

        public async Task<string> UploadProfileImageAsync(string userId, Stream content, string contentType)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_profileImageContainer);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                // Generate unique filename with user ID
                var fileName = $"{userId}_{Guid.NewGuid()}.jpg";
                var blobClient = containerClient.GetBlobClient(fileName);

                // Upload with proper content type
                var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
                await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

                // Generate SAS URL for secure access
                return GenerateSasUrl(blobClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading profile image: {ex.Message}");
                return string.Empty;
            }
        }

        private string GenerateSasUrl(BlobClient blobClient)
        {
            try
            {
                // Check if we can generate SAS tokens (requires account key)
                if (blobClient.CanGenerateSasUri)
                {
                    var sasBuilder = new BlobSasBuilder()
                    {
                        BlobContainerName = blobClient.BlobContainerName,
                        BlobName = blobClient.Name,
                        Resource = "b",
                        StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                        ExpiresOn = DateTimeOffset.UtcNow.AddYears(10) // Long-lived for profile images
                    };

                    sasBuilder.SetPermissions(BlobSasPermissions.Read);

                    var sasUri = blobClient.GenerateSasUri(sasBuilder);
                    return sasUri.ToString();
                }
                else
                {
                    // Fallback to regular URL if SAS cannot be generated
                    return blobClient.Uri.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating SAS URL: {ex.Message}");
                return blobClient.Uri.ToString();
            }
        }

        public async Task<bool> DeleteProfileImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return false;

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_profileImageContainer);

                // Extract filename from URL
                var uri = new Uri(imageUrl);
                var fileName = Path.GetFileName(uri.LocalPath);

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