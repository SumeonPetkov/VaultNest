namespace NestVault.Client.Services
{
    /// <summary>
    /// Holds the current session (JWT + refresh token) in memory and persists
    /// the refresh token and account email in platform secure storage so the
    /// session can be silently restored on the next launch.
    /// </summary>
    public class TokenStore
    {
        private const string RefreshTokenKey = "nestvault_refresh_token";
        private const string EmailKey = "nestvault_email";

        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }
        public string? Email { get; private set; }

        public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

        public async Task SaveSessionAsync(string accessToken, string refreshToken, string email)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            Email = email;

            await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
            await SecureStorage.Default.SetAsync(EmailKey, email);
        }

        public async Task<(string? RefreshToken, string? Email)> GetPersistedSessionAsync()
        {
            try
            {
                var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
                var email = await SecureStorage.Default.GetAsync(EmailKey);
                return (refreshToken, email);
            }
            catch
            {
                // Secure storage can fail if the OS-protected data was reset; treat as logged out.
                return (null, null);
            }
        }

        public Task ClearAsync()
        {
            AccessToken = null;
            RefreshToken = null;
            Email = null;

            SecureStorage.Default.Remove(RefreshTokenKey);
            SecureStorage.Default.Remove(EmailKey);
            return Task.CompletedTask;
        }
    }
}
