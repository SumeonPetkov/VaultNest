using Microsoft.EntityFrameworkCore;
using NestVault.Api.Data;
using NestVault.Api.Interfaces;
using NestVault.Api.Models;
using NestVault.Shared.DTOs.Auth.Requests;
using NestVault.Shared.DTOs.Auth.Responses;

namespace NestVault.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ISessionService _sessionService;

        public AuthService(
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IJwtService jwtService,
            IRefreshTokenService refreshTokenService,
            IPasswordHashService passwordHashService,
            ISessionService sessionService,
            AppDbContext dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _passwordHashService = passwordHashService;
            _sessionService = sessionService;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var exists = await _dbContext.Users
                .AnyAsync(u => u.Email == request.Email);

            if (exists)
            {
                return new RegisterResponse
                {
                    Success = false,
                    ErrorMessage = "User already exists."
                };
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                PasswordHash = _passwordHashService.Hash(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new RegisterResponse
            {
                Success = true
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? deviceName, string? ipAddress, string? userAgent)
        {
            var email = request.Email.Trim();

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password."
                };
            }

            var isValidPassword = _passwordHashService.Verify(request.Password, user.PasswordHash);

            if (!isValidPassword)
            {
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password."
                };
            }

            var refreshToken = _refreshTokenService.GenerateRefreshToken();

            var refreshDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpireDays");
            var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshDays);

            var session = await _sessionService.CreateSessionAsync(
                userId: user.Id,
                refreshToken: refreshToken,
                deviceName: deviceName,
                ipAddress: ipAddress,
                userAgent: userAgent,
                expiresAtUtc: refreshExpiresAt
            );

            var accessToken = _jwtService.GenerateAccessToken(user.Id, session.Id);

            return new LoginResponse
            {
                Success = true,
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var session = await _sessionService.GetValidSessionByRefreshTokenAsync(refreshToken);

            if (session is null)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid or expired refresh token."
                };
            }

            var newRefreshToken = _refreshTokenService.GenerateRefreshToken();

            await _sessionService.RevokeSessionAsync(session.UserId, refreshToken);

            var refreshDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpireDays");
            var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshDays);

            var newSession = await _sessionService.CreateSessionAsync(
                userId: session.UserId,
                refreshToken: newRefreshToken,
                deviceName: session.DeviceName,
                ipAddress: session.IpAddress,
                userAgent: session.UserAgent,
                expiresAtUtc: refreshExpiresAt
            );

            var accessToken = _jwtService.GenerateAccessToken(session.UserId, newSession.Id);

            return new RefreshTokenResponse
            {
                Success = true,
                Token = accessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(Guid userId, string refreshToken)
        {
            await _sessionService.RevokeSessionAsync(userId, refreshToken);
        }

        public Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            throw new NotImplementedException();
        }
    }
}