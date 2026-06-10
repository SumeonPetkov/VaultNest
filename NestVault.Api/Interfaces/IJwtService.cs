using System.Security.Claims;

namespace NestVault.Api.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(Guid userId, Guid sessionId);
        ClaimsPrincipal? ValidateAccessToken(string token);
    }
}