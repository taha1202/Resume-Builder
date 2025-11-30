using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using ResumeBuilder.API.Models; // Add reference to your Models
using System.Net;

namespace ResumeBuilder.API
{
    public class ResumeFunctions
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private const string DatabaseName = "ResumeBuilderDB";
        private const string ContainerName = "Resumes";

        public ResumeFunctions()
        {
            // Retrieves connection string from local.settings.json
            var connectionString = Environment.GetEnvironmentVariable("CosmosConnection");
            _cosmosClient = new CosmosClient(connectionString);
            _container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        }

        // 1. SAVE (Create or Update)
        [Function("SaveResume")]
        public async Task<HttpResponseData> SaveResume(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var resume = JsonConvert.DeserializeObject<Resume>(requestBody);

            // Validation: Ensure ID and UserID exist
            if (string.IsNullOrEmpty(resume.Id)) resume.Id = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(resume.UserId))
            {
                var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReq.WriteStringAsync("UserId is required.");
                return badReq;
            }

            resume.UpdatedAt = DateTime.UtcNow;

            // UPSERT (Updates if exists, Inserts if new)
            await _container.UpsertItemAsync(resume, new PartitionKey(resume.UserId));

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(resume);
            return response;
        }

        // 2. GET ALL (For Dashboard)
        [Function("GetResumes")]
        public async Task<HttpResponseData> GetResumes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "resumes/{userId}")] HttpRequestData req, string userId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId")
                            .WithParameter("@userId", userId);

            var iterator = _container.GetItemQueryIterator<Resume>(query);
            var results = new List<Resume>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            var httpResponse = req.CreateResponse(HttpStatusCode.OK);
            await httpResponse.WriteAsJsonAsync(results);
            return httpResponse;
        }

        // 3. DELETE
        [Function("DeleteResume")]
        public async Task<HttpResponseData> DeleteResume(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "resumes/{userId}/{id}")] HttpRequestData req, string userId, string id)
        {
            try
            {
                await _container.DeleteItemAsync<Resume>(id, new PartitionKey(userId));
                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
        }
    }
}