using NestVault.Api.Data;
using NestVault.Api.Interfaces;
using NestVault.Api.Models.Auth;
using System.Security.Cryptography;

namespace NestVault.Api.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(ILogger<RefreshTokenService> logger)
        {
            _logger = logger;
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
