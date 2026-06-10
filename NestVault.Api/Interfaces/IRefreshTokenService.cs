using NestVault.Api.Models.Auth;

namespace NestVault.Api.Interfaces
{
    public interface IRefreshTokenService
    {
        string GenerateRefreshToken();
    }
}