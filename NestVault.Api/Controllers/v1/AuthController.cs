using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestVault.Api.Interfaces;
using NestVault.Api.Services;
using NestVault.Shared.DTOs.Auth.Requests;
using NestVault.Shared.DTOs.Auth.Responses;
using System.Security.Claims;

namespace NestVault.Api.Controllers.v1
{
    [Route("api/v1/auth")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
        {
            RegisterResponse result = await _authService.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, [FromHeader(Name = "X-Device-Name")] string? deviceName)
        {
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string? userAgent = Request.Headers["User-Agent"].FirstOrDefault();

            LoginResponse result = await _authService.LoginAsync(request, deviceName, ipAddress, userAgent);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
        {
            RefreshTokenResponse result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            await _authService.LogoutAsync(userId, request.RefreshToken);

            return NoContent();
        }
    }
}
