using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Challenge
{
    public class ValidateChallengeDayRequest
    {
        public Guid ChallengeDayId { get; set; }
        public string? Note { get; set; }
    }
}
