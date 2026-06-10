using System;
using System.Collections.Generic;
using System.Text;

namespace NestVault.Shared.DTOs.Auth.Responses
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
