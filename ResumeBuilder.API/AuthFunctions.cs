using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ResumeBuilderFunctions
{
    // FIX: Changed from 'static class' to 'public class' to support Dependency Injection
    public class AuthFunctions
    {
        private readonly ILogger<AuthFunctions> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _usersContainer;

        private const string DatabaseName = "ResumeBuilderDB";
        private const string UsersContainerName = "Users";

        // Note: In a production app, move secrets to KeyVault or App Configuration
        private static readonly string JwtSecret = Environment.GetEnvironmentVariable("JwtSecret") ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        private static readonly int JwtExpirationDays = 7;

        // FIX: Constructor Injection for Logger
        public AuthFunctions(ILogger<AuthFunctions> logger)
        {
            _logger = logger;

            // Initialize Cosmos DB Client (Same pattern as your ResumeFunctions)
            var connectionString = Environment.GetEnvironmentVariable("CosmosConnection");
            _cosmosClient = new CosmosClient(connectionString);
            _usersContainer = _cosmosClient.GetContainer(DatabaseName, UsersContainerName);
        }

        [Function("Signup")]
        public async Task<IActionResult> Signup(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/signup")] HttpRequest req)
        {
            // FIX: Use class-level _logger
            _logger.LogInformation("Signup request received");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var signupData = JsonConvert.DeserializeObject<SignupRequest>(requestBody);

                // Validate input
                if (string.IsNullOrWhiteSpace(signupData?.Email) ||
                    string.IsNullOrWhiteSpace(signupData?.Password) ||
                    string.IsNullOrWhiteSpace(signupData?.Name))
                {
                    return new BadRequestObjectResult(new { message = "Name, email, and password are required" });
                }

                // Check if user already exists
                var query = new QueryDefinition("SELECT * FROM c WHERE c.Email = @email")
                    .WithParameter("@email", signupData.Email.ToLower());

                var existingUsers = _usersContainer.GetItemQueryIterator<User>(query);
                var existingUsersList = new System.Collections.Generic.List<User>();

                while (existingUsers.HasMoreResults)
                {
                    var response = await existingUsers.ReadNextAsync();
                    existingUsersList.AddRange(response);
                }

                if (existingUsersList.Any())
                {
                    return new ConflictObjectResult(new { message = "Email already exists" });
                }

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = Guid.NewGuid().ToString(),
                    Name = signupData.Name,
                    Email = signupData.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(signupData.Password),
                    CreatedAt = DateTime.UtcNow
                };

                // Store user in Cosmos DB
                await _usersContainer.CreateItemAsync(user, new PartitionKey(user.UserId));

                // Generate JWT token
                var token = GenerateJwtToken(user);

                _logger.LogInformation($"User created successfully: {user.Email}");

                return new OkObjectResult(new
                {
                    message = "User created successfully",
                    token = token,
                    user = new
                    {
                        id = user.Id,
                        userId = user.UserId,
                        name = user.Name,
                        email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Signup: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        [Function("Login")]
        public async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequest req)
        {
            _logger.LogInformation("Login request received");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var loginData = JsonConvert.DeserializeObject<LoginRequest>(requestBody);

                // Validate input
                if (string.IsNullOrWhiteSpace(loginData?.Email) ||
                    string.IsNullOrWhiteSpace(loginData?.Password))
                {
                    return new BadRequestObjectResult(new { message = "Email and password are required" });
                }

                // Find user by email
                var query = new QueryDefinition("SELECT * FROM c WHERE c.email = @email")
                    .WithParameter("@email", loginData.Email.ToLower());

                var users = _usersContainer.GetItemQueryIterator<User>(query);
                Console.WriteLine("users = " + users);
                User user = null;

                while (users.HasMoreResults)
                {
                    var response = await users.ReadNextAsync();
                    user = response.FirstOrDefault();
                    if (user != null) break;
                }

                if (user == null)
                {
                    Console.WriteLine("user null");
                    return new UnauthorizedObjectResult(new { message = "Invalid email or password user null" });
                }

                // Verify password
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginData.Password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    Console.WriteLine("invalid");
                    return new UnauthorizedObjectResult(new { message = "Invalid email or password incorrect pass" });
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                _logger.LogInformation($"User logged in successfully: {user.Email}");

                return new OkObjectResult(new
                {
                    message = "Login successful",
                    token = token,
                    user = new
                    {
                        id = user.Id,
                        userId = user.UserId,
                        name = user.Name,
                        email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Login: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        private static string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("userId", user.UserId)
                }),
                Expires = DateTime.UtcNow.AddDays(JwtExpirationDays),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    // Request/Response Models
    public class SignupRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("UserId")]
        public string UserId { get; set; } // Partition key

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("passwordHash")]
        public string PasswordHash { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}