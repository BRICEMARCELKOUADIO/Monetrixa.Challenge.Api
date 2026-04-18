using Monetrixa.ChallengeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        DateTime GetExpirationUtc();
    }
}
