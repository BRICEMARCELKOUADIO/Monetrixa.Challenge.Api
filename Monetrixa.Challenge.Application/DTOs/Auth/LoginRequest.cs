using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
