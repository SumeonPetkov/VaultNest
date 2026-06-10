using System;
using System.Collections.Generic;
using System.Text;

namespace NestVault.Shared.DTOs.Auth.Responses
{
    public class RefreshTokenResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
