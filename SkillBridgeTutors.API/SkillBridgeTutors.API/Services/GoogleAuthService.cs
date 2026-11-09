using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace SkillBridgeTutors.API.Services
{
    /// <summary>
    /// Interface for Google OAuth authentication service
    /// </summary>
    public interface IGoogleAuthService
    {
        /// <summary>
        /// Verifies a Google ID token
        /// </summary>
        Task<GoogleJsonWebSignature.Payload?> VerifyIdTokenAsync(string idToken);

        /// <summary>
        /// Gets user information from Google using access token
        /// </summary>
        Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken);
    }

    /// <summary>
    /// Service for verifying Google OAuth tokens and retrieving user information
    /// </summary>
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;
        private readonly string _clientId;

        public GoogleAuthService(
            IConfiguration configuration,
            ILogger<GoogleAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _clientId = configuration["Google:ClientId"]
                ?? throw new InvalidOperationException("Google:ClientId is not configured in appsettings.json");
        }

        /// <summary>
        /// Verifies the ID token received from Google Sign-In
        /// </summary>
        /// <param name="idToken">The ID token from Google</param>
        /// <returns>Token payload if valid, null otherwise</returns>
        public async Task<GoogleJsonWebSignature.Payload?> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idToken))
                {
                    _logger.LogWarning("Empty ID token provided for verification");
                    return null;
                }

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                _logger.LogInformation($"Successfully verified token for user: {payload.Email}");
                return payload;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Invalid or expired Google token: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error verifying Google token: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves user information from Google OAuth2 API using access token
        /// </summary>
        /// <param name="accessToken">The access token from Google</param>
        /// <returns>User information if successful, null otherwise</returns>
        public async Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    _logger.LogWarning("Empty access token provided");
                    return null;
                }

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Google API returned status {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var userInfo = System.Text.Json.JsonSerializer.Deserialize<GoogleUserInfo>(
                    content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (userInfo != null)
                {
                    _logger.LogInformation($"Successfully retrieved user info for: {userInfo.Email}");
                }

                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get user info from Google: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Google user information from OAuth2 v2 endpoint
    /// </summary>
    public class GoogleUserInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("verified_email")]
        public bool VerifiedEmail { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }
    }
}
