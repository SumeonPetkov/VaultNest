using System;
using System.Collections.Generic;
using System.Text;

namespace NestVault.Shared.DTOs.Auth.Requests
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; }
    }
}
