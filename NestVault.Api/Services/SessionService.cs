using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NestVault.Api.Data;
using NestVault.Api.Interfaces;
using NestVault.Api.Models;

namespace NestVault.Api.Services
{
    public class SessionService : ISessionService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<SessionService> _logger;

        public SessionService(AppDbContext dbContext, ILogger<SessionService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<UserSession> CreateSessionAsync(
            Guid userId,
            string refreshToken,
            string? deviceName,
            string? ipAddress,
            string? userAgent,
            DateTime expiresAtUtc)
        {
            var session = new UserSession
            {
                UserId = userId,
                RefreshTokenHash = HashToken(refreshToken),
                ExpiresAt = expiresAtUtc,
                RevokedAt = null,
                DeviceName = deviceName,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _dbContext.UserSessions.Add(session);
            await _dbContext.SaveChangesAsync();

            return session;
        }

        public async Task<UserSession?> GetValidSessionByRefreshTokenAsync(string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);

            var session = await _dbContext.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s =>
                    s.RefreshTokenHash == tokenHash &&
                    s.RevokedAt == null &&
                    s.ExpiresAt > DateTime.UtcNow);

            return session;
        }

        public async Task RevokeSessionAsync(Guid userId, string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);

            var session = await _dbContext.UserSessions
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId &&
                    s.RefreshTokenHash == tokenHash &&
                    s.RevokedAt == null);

            if (session is null)
                return;

            session.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task RevokeAllSessionsAsync(Guid userId)
        {
            var sessions = await _dbContext.UserSessions
                .Where(s => s.UserId == userId && s.RevokedAt == null)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.RevokedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<UserSession>> GetUserSessionsAsync(Guid userId)
        {
            return await _dbContext.UserSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        private static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}