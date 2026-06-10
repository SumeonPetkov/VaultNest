using System;
using System.Collections.Generic;
using System.Text;

namespace NestVault.Shared.DTOs.Auth.Requests
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
