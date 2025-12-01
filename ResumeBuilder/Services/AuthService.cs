using ResumeBuilder.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace ResumeBuilder.Services
{
    public class AuthService
    {
        private User? _currentUser;
        private string? _authToken;
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly ProtectedSessionStorage _sessionStorage;

        public event Action? OnAuthStateChanged;

        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;
        public string? AuthToken => _authToken;

        public AuthService(
            HttpClient httpClient,
            NavigationManager navigationManager,
            ProtectedSessionStorage sessionStorage)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _sessionStorage = sessionStorage;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var tokenResult = await _sessionStorage.GetAsync<string>("authToken");
                var userResult = await _sessionStorage.GetAsync<User>("currentUser");

                if (tokenResult.Success && userResult.Success)
                {
                    _authToken = tokenResult.Value;
                    _currentUser = userResult.Value;
                    OnAuthStateChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing auth: {ex.Message}");
            }
        }

        public async Task<bool> Login(string email, string password)
        {
            try
            {
                var loginData = new { Email = email, Password = password };
                var response = await _httpClient.PostAsJsonAsync("auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    _authToken = result.Token;
                    _currentUser = result.User;
                    // Store in session
                    await _sessionStorage.SetAsync("authToken", _authToken);
                    await _sessionStorage.SetAsync("currentUser", _currentUser);

                    OnAuthStateChanged?.Invoke();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Signup(string name, string email, string password)
        {
            try
            {
                var signupData = new { Name = name, Email = email, Password = password };
                var response = await _httpClient.PostAsJsonAsync("auth/signup", signupData);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    _authToken = result.Token;
                    _currentUser = result.User;

                    // Store in session
                    await _sessionStorage.SetAsync("authToken", _authToken);
                    await _sessionStorage.SetAsync("currentUser", _currentUser);

                    OnAuthStateChanged?.Invoke();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Signup error: {ex.Message}");
                return false;
            }
        }

        public async Task Logout()
        {
            _currentUser = null;
            _authToken = null;

            // Clear session
            await _sessionStorage.DeleteAsync("authToken");
            await _sessionStorage.DeleteAsync("currentUser");

            OnAuthStateChanged?.Invoke();
            _navigationManager.NavigateTo("/", forceLoad: true);
        }

        public async Task<bool> UpdateUserProfile(string name, string profileImageUrl)
        {
            try
            {
                if (_currentUser == null || string.IsNullOrEmpty(_authToken))
                {
                    return false;
                }

                var updateData = new
                {
                    UserId = _currentUser.UserId,
                    Name = name,
                    ProfileImageUrl = profileImageUrl
                };

                var request = new HttpRequestMessage(HttpMethod.Put, "auth/update-profile");
                request.Headers.Add("Authorization", $"Bearer {_authToken}");
                request.Content = JsonContent.Create(updateData);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UpdateProfileResponse>();

                    // Update current user
                    _currentUser.Name = result.User.Name;
                    _currentUser.ProfileImageUrl = result.User.ProfileImageUrl;

                    // Update session storage
                    await _sessionStorage.SetAsync("currentUser", _currentUser);

                    OnAuthStateChanged?.Invoke();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update profile error: {ex.Message}");
                return false;
            }
        }
    }

    public class AuthResponse
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public User User { get; set; }
    }

    public class UpdateProfileResponse
    {
        public string Message { get; set; }
        public UserResponse User { get; set; }
    }

    public class UserResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}