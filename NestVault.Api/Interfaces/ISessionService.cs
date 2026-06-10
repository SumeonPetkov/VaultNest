using NestVault.Api.Models;

namespace NestVault.Api.Interfaces
{
    public interface ISessionService
    {
        Task<UserSession> CreateSessionAsync(
            Guid userId,
            string refreshToken,
            string? deviceName,
            string? ipAddress,
            string? userAgent,
            DateTime expiresAtUtc);

        Task<UserSession?> GetValidSessionByRefreshTokenAsync(string refreshToken);

        Task RevokeSessionAsync(Guid userId, string refreshToken);

        Task RevokeAllSessionsAsync(Guid userId);

        Task<List<UserSession>> GetUserSessionsAsync(Guid userId);
    }
}