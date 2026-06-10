using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using NestVault.Shared.DTOs.Auth.Requests;
using NestVault.Shared.DTOs.Auth.Responses;

namespace NestVault.Client.Services
{
    public record AuthResult(bool Success, string? ErrorMessage);

    /// <summary>
    /// Client for the NestVault API auth endpoints (api/v1/auth).
    /// </summary>
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStore _tokenStore;
        private readonly ILogger<AuthApiService> _logger;

        public AuthApiService(HttpClient httpClient, TokenStore tokenStore, ILogger<AuthApiService> logger)
        {
            _httpClient = httpClient;
            _tokenStore = tokenStore;
            _logger = logger;
        }

        public async Task<AuthResult> RegisterAsync(string firstName, string lastName, string email, string password, string confirmPassword)
        {
            try
            {
                var request = new RegisterRequest
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Password = password,
                    ConfirmPassword = confirmPassword
                };

                var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", request);
                var result = await ReadJsonSafeAsync<RegisterResponse>(response);

                if (result is null)
                    return new AuthResult(false, $"The server returned an unexpected response ({(int)response.StatusCode}).");

                return new AuthResult(result.Success, result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register request failed");
                return new AuthResult(false, "Could not reach the server. Is the API running?");
            }
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/login")
                {
                    Content = JsonContent.Create(new LoginRequest { Email = email, Password = password })
                };
                message.Headers.Add("X-Device-Name", DeviceInfo.Name);

                var response = await _httpClient.SendAsync(message);
                var result = await ReadJsonSafeAsync<LoginResponse>(response);

                if (result is null || (result.Success && (result.Token is null || result.RefreshToken is null)))
                    return new AuthResult(false, $"The server returned an unexpected response ({(int)response.StatusCode}).");

                if (!result.Success)
                    return new AuthResult(false, result.ErrorMessage ?? "Invalid email or password.");

                await _tokenStore.SaveSessionAsync(result.Token!, result.RefreshToken!, email);
                return new AuthResult(true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login request failed");
                return new AuthResult(false, "Could not reach the server. Is the API running?");
            }
        }

        /// <summary>
        /// Attempts to restore the previous session using the persisted refresh token.
        /// </summary>
        public async Task<bool> TryRestoreSessionAsync()
        {
            var (refreshToken, email) = await _tokenStore.GetPersistedSessionAsync();
            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(email))
                return false;

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/v1/auth/refresh-token",
                    new RefreshTokenRequest { RefreshToken = refreshToken });

                var result = await ReadJsonSafeAsync<RefreshTokenResponse>(response);
                if (result is null || !result.Success || result.Token is null || result.RefreshToken is null)
                    return false;

                await _tokenStore.SaveSessionAsync(result.Token, result.RefreshToken, email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token request failed");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                if (_tokenStore.AccessToken is not null && _tokenStore.RefreshToken is not null)
                {
                    using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/logout")
                    {
                        Content = JsonContent.Create(new LogoutRequest { RefreshToken = _tokenStore.RefreshToken })
                    };
                    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStore.AccessToken);

                    await _httpClient.SendAsync(message);
                }
            }
            catch (Exception ex)
            {
                // Revoking the server session is best effort; the local session is cleared regardless.
                _logger.LogWarning(ex, "Logout request failed");
            }
            finally
            {
                await _tokenStore.ClearAsync();
            }
        }

        /// <summary>
        /// Reads the response body as JSON, returning default when the server
        /// answered with something else (e.g. an HTML error page).
        /// </summary>
        private static async Task<T?> ReadJsonSafeAsync<T>(HttpResponseMessage response)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch
            {
                return default;
            }
        }
    }
}
