using NestVault.Shared.DTOs.Auth.Requests;
using NestVault.Shared.DTOs.Auth.Responses;

namespace NestVault.Api.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request, string? deviceName, string? ipAddress, string? userAgent);
        Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(Guid userId, string refreshToken);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}
